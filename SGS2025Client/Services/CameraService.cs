using JerryShaw.HCNet;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace SGS2025Client.Services
{
    public class CameraService
    {
        private int _userID = -1;
        private int _realHandle = -1;
        private int _playPort = -1;

        private CHCNetSDK.REALDATACALLBACK _realDataCallback;
        private PlayCtrl.DECCBFUN _decCallback;

        private string _latestBase64Image;
        private object _lock = new();

        private readonly string _ip = "192.168.1.107";
        private readonly int _port = 8002;
        private readonly string _username = "admin";
        private readonly string _password = "abcd@1234";

        public CameraService()
        {
            if (!CHCNetSDK.NET_DVR_Init())
                throw new Exception("HCNetSDK init failed.");
        }

        public void Init()
        {
            if (_userID < 0)
            {
                CHCNetSDK.NET_DVR_DEVICEINFO_V30 devInfo = new();
                _userID = CHCNetSDK.NET_DVR_Login_V30(_ip, _port, _username, _password, ref devInfo);
                if (_userID < 0)
                    throw new Exception("Login failed. Error code: " + CHCNetSDK.NET_DVR_GetLastError());
            }

            if (!PlayCtrl.PlayM4_GetPort(ref _playPort))
                throw new Exception("Get PlayM4 port failed");

            PlayCtrl.PlayM4_SetStreamOpenMode(_playPort, 0); // real-time
            PlayCtrl.PlayM4_OpenStream(_playPort, IntPtr.Zero, 0, 8 * 1024 * 1024);

          //  _decCallback = new PlayCtrl.DECCBFUN(DecodeCallback);
           // PlayCtrl.PlayM4_SetDecCallBack(_playPort, _decCallback);
         //   PlayCtrl.PlayM4_SetDecCallBackEx(_playPort, _decCallback, IntPtr.Zero, 0);
            PlayCtrl.PlayM4_Play(_playPort, IntPtr.Zero); // Không hiển thị UI

            _realDataCallback = new CHCNetSDK.REALDATACALLBACK(RealDataCallback);

            var previewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO
            {
                lChannel = 1,
                dwStreamType = 0,
                dwLinkMode = 0,
                bBlocked = true
            };

            _realHandle = CHCNetSDK.NET_DVR_RealPlay_V40(_userID, ref previewInfo, _realDataCallback, IntPtr.Zero);
            if (_realHandle < 0)
                throw new Exception("RealPlay failed. Error code: " + CHCNetSDK.NET_DVR_GetLastError());
        }
        public string GetBase64Image()
        {
            try
            {
                int jpegSize = 8*1024 * 1024; // 1MB
                byte[] jpegBuffer = new byte[jpegSize];
                uint actualSize = 0;

                // Ghim mảng lại để lấy con trỏ
                GCHandle handle = GCHandle.Alloc(jpegBuffer, GCHandleType.Pinned);
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(jpegBuffer, 0);

                bool result = PlayCtrl.PlayM4_GetJPEG(_playPort, ptr, (uint)jpegSize, ref actualSize);
                handle.Free();

                if (!result || actualSize == 0)
                {
                    Console.WriteLine("Lấy JPEG thất bại, lỗi: " + PlayCtrl.PlayM4_GetLastError(_playPort));
                    return null;
                }

                var base64 = "data:image/jpeg;base64," + Convert.ToBase64String(jpegBuffer, 0, (int)actualSize);
                lock (_lock)
                {
                    _latestBase64Image = base64;
                }

                return base64;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lấy ảnh JPEG: " + ex.Message);
                return null;
            }
        }
        private void RealDataCallback(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser)
        {
            if (dwDataType == CHCNetSDK.NET_DVR_STREAMDATA && dwBufSize > 0)
            {
                PlayCtrl.PlayM4_InputData(_playPort, pBuffer, dwBufSize);
            }
        }

        private void DecodeCallback(int nPort, IntPtr pBuf, int nSize, ref PlayCtrl.FRAME_INFO pFrameInfo, int nUser, int nReserved2)
        {
            var t = pFrameInfo.nWidth * pFrameInfo.nHeight * 3;
            Console.WriteLine($"{nSize} , {pFrameInfo.nWidth * pFrameInfo.nHeight * 3}");
            if (pFrameInfo.nType != 3 || pBuf == IntPtr.Zero)
            {
                return; // Loại bỏ frame sai
            }
            int expectedMinSize = pFrameInfo.nWidth * pFrameInfo.nHeight * 3;
            if (nSize < expectedMinSize / 2) // chỉ reject nếu quá nhỏ
                return;

            try
            {
                int width = pFrameInfo.nWidth;
                int height = pFrameInfo.nHeight;
                int bytesPerPixel = 3;
                int stride = width * bytesPerPixel;

                using Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    bmp.PixelFormat);

                unsafe
                {
                    byte* srcBase = (byte*)pBuf.ToPointer();
                    byte* destBase = (byte*)bmpData.Scan0.ToPointer();

                    for (int y = 0; y < height; y++)
                    {
                        byte* srcLine = srcBase + y * stride;
                        byte* destLine = destBase + y * bmpData.Stride;

                        int copyBytes = Math.Min(stride, bmpData.Stride); // thêm dòng này
                        Buffer.MemoryCopy(srcLine, destLine, bmpData.Stride, copyBytes);
                    }
                }

                bmp.UnlockBits(bmpData);

                // Convert Bitmap to Base64 string (JPEG)
                using MemoryStream ms = new();
                bmp.Save(ms, ImageFormat.Jpeg);
                string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());

                lock (_lock)
                {
                    _latestBase64Image = base64;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decode error: {ex.Message}");
            }
        }

        //public string GetBase64Image()
        //{
        //    lock (_lock)
        //    {
        //        return _latestBase64Image;
        //    }
        //}

        public void Stop()
        {
            if (_realHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(_realHandle);
                _realHandle = -1;
            }

            if (_userID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(_userID);
                _userID = -1;
            }

            if (_playPort >= 0)
            {
                PlayCtrl.PlayM4_Stop(_playPort);
                PlayCtrl.PlayM4_CloseStream(_playPort);
                //PlayCtrl.PlayM4_ReleasePort(_playPort);
                _playPort = -1;
            }

            CHCNetSDK.NET_DVR_Cleanup();
        }

    }

}
