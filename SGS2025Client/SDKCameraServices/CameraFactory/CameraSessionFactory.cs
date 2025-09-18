using CMS_Data.Enums;
using SGS2025Client.SDKCameraServices.Dahua;
using SGS2025Client.SDKCameraServices.Hik;
using SGS2025Client.SDKCameraServices.Tvt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.SDKCameraServices.CameraFactory
{
    
    public static class CameraSessionFactory
    {
        public static ICameraSession Create(CameraType type, string ip, int port, string user, string pass)
        {
            return type switch
            {
                CameraType.Hikvision => new HikvisionCameraSession(ip, port, user, pass),
                CameraType.Dahua => new DahuaCameraSession(ip, port, user, pass),
                CameraType.Tvt => new TvtCameraSession(ip, port, user, pass),
                _ => throw new NotSupportedException("Loại camera chưa hỗ trợ")
            };
        }
    }
}
