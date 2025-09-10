using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.SDKCameraServices.CameraFactory
{
    public class FactoryCameraService
    {
        private readonly Dictionary<string, ICameraSession> _cameras = new();

        public void AddCamera(string id, CameraType type, string ip, int port, string username, string password)
        {
            if (_cameras.ContainsKey(id)) return;

            var cam = CameraSessionFactory.Create(type, ip, port, username, password);
            cam.Init();
            _cameras[id] = cam;
        }

        public string GetImage(string id) =>
            _cameras.TryGetValue(id, out var cam) ? cam.GetBase64Image() : null;

        public string CaptureToUrl(string id) =>
            _cameras.TryGetValue(id, out var cam) ? cam.CaptureToUrl(id) : null;

        public void StopAll()
        {
            foreach (var cam in _cameras.Values)
                cam.Stop();
        }
    }
}
