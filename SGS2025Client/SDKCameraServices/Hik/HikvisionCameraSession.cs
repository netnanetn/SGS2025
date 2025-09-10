using JerryShaw.HCNet;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.SDKCameraServices.Hik
{
    public class HikvisionCameraSession
    {
        private int _userID = -1;
        private int _realHandle = -1;
        private int _playPort = -1;

        private string _ip;
        private int _port;
        private string _username;
        private string _password;

        private string _latestBase64Image;
        private object _lock = new();

        private CHCNetSDK.REALDATACALLBACK _realDataCallback;
        private PlayCtrl.DISPLAYCBFUN _displayCallback;

        private readonly string _imageFolder = @"C:\TVS\Images\temp";
        private byte[] _latestByteImage;

        public HikvisionCameraSession(string ip, int port, string username, string password)
        {
            _ip = ip;
            _port = port;
            _username = username;
            _password = password;
        }

        public void Init()
        {
            if (!CHCNetSDK.NET_DVR_Init())
                throw new Exception("NET_DVR_Init failed.");

            var devInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            _userID = CHCNetSDK.NET_DVR_Login_V30(_ip, _port, _username, _password, ref devInfo);
            if (_userID < 0)
                throw new Exception("Login failed: " + CHCNetSDK.NET_DVR_GetLastError());

            if (!PlayCtrl.PlayM4_GetPort(ref _playPort))
                throw new Exception("PlayM4_GetPort failed");
            //PlayCtrl.PlayM4_SetStreamOpenMode(_playPort, 0);
            //PlayCtrl.PlayM4_OpenStream(_playPort, nint.Zero, 0, 8 * 1024 * 1024);
            //PlayCtrl.PlayM4_Play(_playPort, nint.Zero);


            


            _realDataCallback = new CHCNetSDK.REALDATACALLBACK(RealDataCallback);

            var previewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO
            {
                lChannel = 1,
                dwStreamType = 1,
                dwLinkMode = 0,
                bBlocked = true,
                hPlayWnd = IntPtr.Zero,
            }; 
            _realHandle = CHCNetSDK.NET_DVR_RealPlay_V40(_userID, ref previewInfo, _realDataCallback , nint.Zero);
            if (_realHandle < 0)
                throw new Exception("RealPlay failed: " + CHCNetSDK.NET_DVR_GetLastError());
        }

        private void RealDataCallback(int lRealHandle, uint dwDataType, nint pBuffer, uint dwBufSize, nint pUser)
        {
            if (dwDataType == CHCNetSDK.NET_DVR_SYSHEAD)
            {
                PlayCtrl.PlayM4_SetStreamOpenMode(_playPort, 0);
                PlayCtrl.PlayM4_OpenStream(_playPort, pBuffer, dwBufSize, 8 * 1024 * 1024);
                PlayCtrl.PlayM4_Play(_playPort, nint.Zero);
                // Đăng ký callback để lấy frame
              //  _displayCallback = OnDisplayFrame;
             //   PlayCtrl.PlayM4_SetDisplayCallBack(_playPort, _displayCallback);
            }
            else
            {
                PlayCtrl.PlayM4_InputData(_playPort, pBuffer, dwBufSize);
            }
        }
        private void OnDisplayFrame(int nPort,
        IntPtr pBuf,
        int nSize,
        int nWidth,
        int nHeight,
        int nStamp,
        int nType,
        int nReserved)
        {
            try
            {
                var logt = $"nType={nType}, nSize={nSize}, expected={nWidth * nHeight * 3}, width={nWidth}, height={nHeight}";
                if (nSize <= 0 || pBuf == IntPtr.Zero)
                    return;

                byte[] buffer = new byte[nSize];
                Marshal.Copy(pBuf, buffer, 0, nSize);

                byte[] rgb = null;

                // Detect theo nType + nSize
                int rgb24 = nWidth * nHeight * 3;
                int rgb32 = nWidth * nHeight * 4;
                int yuv420 = nWidth * nHeight * 3 / 2;
                int yuv422 = nWidth * nHeight * 2;

                if (nType == 3 && nSize == yuv420) // thực chất YV12
                {
                    rgb = Yv12ToRgb24(buffer, nWidth, nHeight);
                }
                else if (nType == 1 || nSize == yuv422) // YUY2
                {
                    rgb = Yuy2ToRgb24(buffer, nWidth, nHeight);
                }
                else if (nType == 2 || nSize == rgb32) // RGB32
                {
                    rgb = new byte[rgb24];
                    for (int i = 0, j = 0; i < nSize; i += 4, j += 3)
                    {
                        rgb[j] = buffer[i];       // B
                        rgb[j + 1] = buffer[i + 1]; // G
                        rgb[j + 2] = buffer[i + 2]; // R
                    }
                }
                else if (nType == 3 && nSize == rgb24) // RGB24 chuẩn
                {
                    rgb = buffer;
                }
                else
                {
                    Console.WriteLine($"Không nhận diện được frame: nType={nType}, nSize={nSize}");
                    return;
                }

                // Convert RGB24 -> Bitmap
                using var bmp = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);
                var bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);

                int stride = bmpData.Stride;
                int srcStride = nWidth * 3;

                for (int y = 0; y < nHeight; y++)
                {
                    IntPtr dstPtr = bmpData.Scan0 + y * stride;
                    int srcIndex = y * srcStride;
                    Marshal.Copy(rgb, srcIndex, dstPtr, srcStride);
                }

                bmp.UnlockBits(bmpData);

                using var ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                lock (_lock)
                {
                    _latestByteImage = ms.ToArray();
                }


            }
            catch(Exception e)
            {
                // ignore frame errors
            }
        }
        public string GetBase64Image()
        {
            try
            {
                int jpegSize = 8 * 1024 * 1024;
                byte[] buffer = new byte[jpegSize];
                uint actualSize = 0;

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                nint ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

                bool result = PlayCtrl.PlayM4_GetJPEG(_playPort, ptr, (uint)jpegSize, ref actualSize);
                handle.Free();

                if (!result || actualSize == 0)
                    return null;

                string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(buffer, 0, (int)actualSize);
                lock (_lock)
                {
                    _latestBase64Image = base64;
                }

                return base64;
            }
            catch
            {
                return null;
            }
        }
        public string CaptureToUrl2(string camId)
        {
            try
            {
                if (_latestByteImage == null || _latestByteImage.Length == 0) return null;

                string finalPath = Path.Combine(_imageFolder, $"{camId}.jpg");
                string tempPath = finalPath + ".tmp";

                // Ghi ra file tạm
                File.WriteAllBytes(tempPath, _latestByteImage);

                // Đổi tên file tạm -> file chính (atomic, không bị denied)
                File.Move(tempPath, finalPath, true);
            }
            catch (Exception ex)
            {
                //  Console.WriteLine($"❌ SaveCameraImage error {camId}: {ex.Message}");
            }
            return $"https://local.tvs/temp/{camId}.jpg";
            try
            {
                int jpegSize = 8 * 1024 * 1024;
                byte[] buffer = new byte[jpegSize];
                uint actualSize = 0;

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                nint ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

                bool result = PlayCtrl.PlayM4_GetJPEG(_playPort, ptr, (uint)jpegSize, ref actualSize);
                handle.Free();

                if (!result || actualSize == 0)
                    return null;

                // Ghi file
                string filePath = Path.Combine(_imageFolder, $"{camId}.jpg");
                File.WriteAllBytes(filePath, buffer.Take((int)actualSize).ToArray());

                // Trả về URL cho Blazor
                return $"https://local.tvs/temp/{camId}.jpg";
            }
            catch
            {
                return null;
            }
        }
        public string CaptureToUrl(string camId)
        {
            
            try
            {
                int jpegSize = 8 * 1024 * 1024;
                byte[] buffer = new byte[jpegSize];
                uint actualSize = 0;

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                nint ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

                bool result = PlayCtrl.PlayM4_GetJPEG(_playPort, ptr, (uint)jpegSize, ref actualSize);
                handle.Free();

                if (!result || actualSize == 0)
                    return null;

                // Ghi file
                string filePath = Path.Combine(_imageFolder, $"{camId}.jpg");
                File.WriteAllBytes(filePath, buffer.Take((int)actualSize).ToArray());

                // Trả về URL cho Blazor
                return $"https://local.tvs/temp/{camId}.jpg";
            }
            catch
            {
                return null;
            }
        }
        private static byte[] Yv12ToRgb24(byte[] yv12, int width, int height)
        {
            int frameSize = width * height;
            int qFrameSize = frameSize / 4;

            byte[] rgb = new byte[frameSize * 3];

            int yIndex = 0;
            int vIndex = frameSize;
            int uIndex = frameSize + qFrameSize;

            int rgbIndex = 0;

            for (int j = 0; j < height; j++)
            {
                int uvRow = (j / 2) * (width / 2);
                for (int i = 0; i < width; i++)
                {
                    int y = yv12[yIndex++] & 0xff;
                    int v = yv12[vIndex + uvRow + (i / 2)] & 0xff;
                    int u = yv12[uIndex + uvRow + (i / 2)] & 0xff;

                    int c = y - 16;
                    int d = u - 128;
                    int e = v - 128;

                    int r = (298 * c + 409 * e + 128) >> 8;
                    int g = (298 * c - 100 * d - 208 * e + 128) >> 8;
                    int b = (298 * c + 516 * d + 128) >> 8;

                    rgb[rgbIndex++] = (byte)Math.Clamp(b, 0, 255);
                    rgb[rgbIndex++] = (byte)Math.Clamp(g, 0, 255);
                    rgb[rgbIndex++] = (byte)Math.Clamp(r, 0, 255);
                }
            }

            return rgb;
        }
        private static byte[] Yuy2ToRgb24(byte[] yuy2, int width, int height)
        {
            byte[] rgb = new byte[width * height * 3];
            int rgbIndex = 0;

            for (int i = 0; i < yuy2.Length; i += 4)
            {
                byte y0 = yuy2[i];
                byte u = yuy2[i + 1];
                byte y1 = yuy2[i + 2];
                byte v = yuy2[i + 3];

                rgbIndex = ConvertYuvToRgb(y0, u, v, rgb, rgbIndex);
                rgbIndex = ConvertYuvToRgb(y1, u, v, rgb, rgbIndex);
            }

            return rgb;
        }

        private static int ConvertYuvToRgb(int y, int u, int v, byte[] rgb, int index)
        {
            int c = y - 16;
            int d = u - 128;
            int e = v - 128;

            int r = (298 * c + 409 * e + 128) >> 8;
            int g = (298 * c - 100 * d - 208 * e + 128) >> 8;
            int b = (298 * c + 516 * d + 128) >> 8;

            rgb[index++] = (byte)Math.Clamp(b, 0, 255);
            rgb[index++] = (byte)Math.Clamp(g, 0, 255);
            rgb[index++] = (byte)Math.Clamp(r, 0, 255);

            return index;
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

            PlayCtrl.PlayM4_Stop(_playPort);
            PlayCtrl.PlayM4_CloseStream(_playPort);
            PlayCtrl.PlayM4_FreePort(_playPort);
        }
    }
}
