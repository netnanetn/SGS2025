#if WINDOWS
using QuickNV.DahuaNetSDK.Api;
using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace SGS2025Client.SDKCameraServices.Dahua
{
    public class DahuaCameraService2
    {
        private Dictionary<string, DahuaCameraSession2> _cameras = new();

        public void AddCamera(string id, string ip, int port, string username, string password, IntPtr hwnd, int channel = 0)
        {
            if (_cameras.ContainsKey(id)) return;

            var cam = new DahuaCameraSession2(ip, port, username, password);
            cam.Init();
            cam.StartRealPlayWithHwnd(hwnd, 0);
            //  cam.StartLive(0); // Gọi phát live luôn
            _cameras[id] = cam;
        }
      
        public string GetImage(string id)
        {
            if (_cameras.ContainsKey(id))
                return _cameras[id].GetBase64Image();
            return null;
        }

        public void StopAll()
        {
            foreach (var cam in _cameras.Values)
                cam.Stop();
        }
        public void StopCamera(string id)
        {
            if (_cameras.TryGetValue(id, out var cam))
            {
                cam.StopRealPlay();
                cam.Dispose();
                _cameras.Remove(id);
            }
        }
    }
}
#endif
