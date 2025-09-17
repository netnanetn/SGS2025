using SGS2025Client.SDKCameraServices.CameraFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SGS2025Client.Models
{
 
    public class AppConfig
    {
        public SystemConfig System { get; set; }
        public PrintConfig Print { get; set; }
        public ComConfig ComConfig { get; set; }
        public ReadComConfig ReadCom { get; set; }
        public List<BarrierConfig> Barrier { get; set; }
        public CameraConfig Camera { get; set; }
    }

    public class SystemConfig
    {
        public bool UseBarrier { get; set; }
        public bool UseCamera { get; set; }
        public bool UseCard { get; set; }
        public bool UseSensor { get; set; }
        public bool UseLamp { get; set; }
        public bool UseSpeak { get; set; }
        public bool AutoScale { get; set; }
        public bool LockScale { get; set; }
    }

    public class PrintConfig
    {
        public bool NotView { get; set; }
        public int NumCopies { get; set; }
        public bool NoControl { get; set; }
        public string ScalePdfTemplate { get; set; }
    }

    public class ComConfig
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public string Parity { get; set; }
        public int DataBits { get; set; }
        public int StopBits { get; set; }
    }

    public class ReadComConfig
    {
        public string ReadMethod { get; set; }
        public int LengthStart { get; set; }
        public int LengthNumber { get; set; }
        public int LengthSubstring { get; set; }
        public string ProcessType { get; set; }
        public string ShowTextMethod { get; set; }
        public int ReceivedLine { get; set; }
    }

    public class BarrierConfig
    {
        public string Ip { get; set; }
        public int Port { get; set; }
    }

    public class CameraConfig
    {
        public List<CameraItem> List { get; set; }
    }

    public class CameraItem
    {
        public string Code { get; set; }
        //public string TypeCamera { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CameraType TypeCamera { get; set; }
        public int DauGhiPort { get; set; }
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int Channel { get; set; }
        public bool Capture { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

}
