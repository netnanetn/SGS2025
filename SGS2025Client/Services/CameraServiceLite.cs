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
    public class CameraServiceLite
    {
        private int _userID = -1;
        private int _realHandle = -1;
        private int _playPort = -1;

        private readonly string _ip = "192.168.1.107";
        private readonly int _port = 8002;
        private readonly string _username = "admin";
        private readonly string _password = "abcd@1234";

        private readonly object _lock = new();
        private string _latestBase64Image;

        private CHCNetSDK.REALDATACALLBACK _realDataCallback;
        public CameraServiceLite()
        {
            if (!CHCNetSDK.NET_DVR_Init())
                throw new Exception("HCNetSDK init failed.");
        }
        public void Init()
        {
            if (!CHCNetSDK.NET_DVR_Init())
                throw new Exception("NET_DVR_Init failed.");

            if (_userID < 0)
            {
                var devInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
                _userID = CHCNetSDK.NET_DVR_Login_V30(_ip, _port, _username, _password, ref devInfo);
                if (_userID < 0)
                    throw new Exception("Login failed. Error code: " + CHCNetSDK.NET_DVR_GetLastError());
            }

            if (!PlayCtrl.PlayM4_GetPort(ref _playPort))
                throw new Exception("Get PlayM4 port failed");

            PlayCtrl.PlayM4_SetStreamOpenMode(_playPort, 0); // real-time
            PlayCtrl.PlayM4_OpenStream(_playPort, IntPtr.Zero, 0, 2 * 1024 * 1024); // 2MB buffer
            PlayCtrl.PlayM4_Play(_playPort, IntPtr.Zero); // No display

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
                throw new Exception("RealPlay failed. Error: " + CHCNetSDK.NET_DVR_GetLastError());
        }

        private void RealDataCallback(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser)
        {
            if (dwDataType == CHCNetSDK.NET_DVR_SYSHEAD)
            {
                PlayCtrl.PlayM4_OpenStream(_playPort, pBuffer, dwBufSize, 2 * 1024 * 1024);
                PlayCtrl.PlayM4_Play(_playPort, IntPtr.Zero);
            }
            else
            {
                PlayCtrl.PlayM4_InputData(_playPort, pBuffer, dwBufSize);
            }
        }

        public string GetBase64Image()
        {
            try
            {
                int jpegSize = 8 * 1024 * 1024;
                byte[] jpegBuffer = new byte[jpegSize];
                uint actualSize = 0;

                GCHandle handle = GCHandle.Alloc(jpegBuffer, GCHandleType.Pinned);
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(jpegBuffer, 0);

                bool result = PlayCtrl.PlayM4_GetJPEG(_playPort, ptr, (uint)jpegSize, ref actualSize);
                handle.Free();

                if (!result || actualSize == 0)
                {
                    Console.WriteLine("Lấy JPEG thất bại, lỗi: " + PlayCtrl.PlayM4_GetLastError(_playPort));
                    return null;
                }

                string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(jpegBuffer, 0, (int)actualSize);
                lock (_lock)
                {
                    _latestBase64Image = base64;
                }

                return base64;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetBase64Image: " + ex.Message);
                return null;
            }
        }
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
