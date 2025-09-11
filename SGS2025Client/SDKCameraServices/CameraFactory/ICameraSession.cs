using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.SDKCameraServices.CameraFactory
{
    public interface ICameraSession
    {
        void Init();
        string GetBase64Image();
        string CaptureToUrl(string id);
        void Stop();
    }
}
