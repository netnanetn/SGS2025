#if WINDOWS
using QuickNV.DahuaNetSDK.Api;
using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace SGS2025Client.SDKCameraServices.Dahua
{
    public class DahuaCameraService
    {
        private Dictionary<string, DahuaCameraSession> _cameras = new();

        public void AddCamera(string id, string ip, int port, string username, string password)
        {
            if (_cameras.ContainsKey(id)) return;

            var cam = new DahuaCameraSession(ip, port, username, password);
            cam.Init();
          //  cam.StartLive(0); // Gọi phát live luôn
            _cameras[id] = cam;
        }
      
        public string GetImage(string id)
        {
            if (_cameras.ContainsKey(id))
                return _cameras[id].GetBase64Image();
            return null;
        }
        public string GetImageBase64(string id)
        {
            if (_cameras.ContainsKey(id))
                return _cameras[id].GetBase64ImageByConvert();
            return null;
        }
        //public byte[] GetImageBytes(string id)
        //{
        //    if (_cameras.ContainsKey(id))
        //        return _cameras[id].GetBase64Image();
        //    return null;
        //}
        public void StopAll()
        {
            foreach (var cam in _cameras.Values)
                cam.Stop();
        }
    }
}
#endif
