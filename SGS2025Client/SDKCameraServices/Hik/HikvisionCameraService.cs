using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.SDKCameraServices.Hik
{
    public class HikvisionCameraService
    {
        private Dictionary<string, HikvisionCameraSession> _cameras = new();

        public void AddCamera(string id, string ip, int port, string username, string password)
        {
            if (_cameras.ContainsKey(id)) return;

            var cam = new HikvisionCameraSession(ip, port, username, password);
            cam.Init();
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
    }
}
