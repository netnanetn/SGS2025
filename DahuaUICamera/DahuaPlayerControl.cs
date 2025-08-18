using System;
using System.ComponentModel;
using System.Windows.Forms;
using NetSDKCS; // nếu bạn dùng NetSDKCS
// Hoặc using QuickNV.DahuaNetSDK; using QuickNV.DahuaNetSDK.Api;

namespace DahuaNativeViewer
{
    public class DahuaPlayerControl : Panel
    {
        private IntPtr _login = IntPtr.Zero;
        private IntPtr _real = IntPtr.Zero;

        [Browsable(true)]
        public string CameraIp { get; set; } = "192.168.1.109";
        [Browsable(true)]
        public int CameraPort { get; set; } = 37777;
        [Browsable(true)]
        public string Username { get; set; } = "admin";
        [Browsable(true)]
        public string Password { get; set; } = "abcd@1234";
        [Browsable(true)]
        public int Channel { get; set; } = 0;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                NETClient.Init(null, IntPtr.Zero, null);
            }
            catch { }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Stop();
            try { NETClient.Cleanup(); } catch { }
            base.OnHandleDestroyed(e);
        }

        public void Start()
        {
            if (_login != IntPtr.Zero) return;

            var devInfo = new NET_DEVICEINFO_Ex();
            _login = NETClient.Login(CameraIp, (ushort)CameraPort, Username, Password,
                                     EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref devInfo);
            if (_login == IntPtr.Zero)
                throw new Exception("Login failed: " + NETClient.GetLastError());

            _real = NETClient.RealPlay(_login, Channel, this.Handle);
            if (_real == IntPtr.Zero)
                throw new Exception("RealPlay failed: " + NETClient.GetLastError());
        }

        public void Stop()
        {
            if (_real != IntPtr.Zero)
            {
                NETClient.StopRealPlay(_real);
                _real = IntPtr.Zero;
            }
            if (_login != IntPtr.Zero)
            {
                NETClient.Logout(_login);
                _login = IntPtr.Zero;
            }
        }

        public bool CaptureJpeg()
        {
            if (_real == IntPtr.Zero) return false;
            var snapParams = new NET_SNAP_PARAMS
            {
                Channel = 0,
                Quality = 100,
                ImageSize = 0,
                mode = 0,
                InterSnap = 0,
                CmdSerial = 0,
                Reserved = new uint[4]
            };
            // Tuỳ SDK: một số SDK Dahua có hàm capture trực tiếp từ real handle
            // Ở đây minh hoạ dùng NETClient.CapturePicture (tuỳ wrapper)
            return NETClient.SnapPictureEx(_real, snapParams, IntPtr.Zero);
        }
    }
}
