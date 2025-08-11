using JerryShaw.HCNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.SDKCameraServices
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

            PlayCtrl.PlayM4_SetStreamOpenMode(_playPort, 0);
            PlayCtrl.PlayM4_OpenStream(_playPort, IntPtr.Zero, 0, 2 * 1024 * 1024);
            PlayCtrl.PlayM4_Play(_playPort, IntPtr.Zero);

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
                throw new Exception("RealPlay failed: " + CHCNetSDK.NET_DVR_GetLastError());
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
                byte[] buffer = new byte[jpegSize];
                uint actualSize = 0;

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

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
