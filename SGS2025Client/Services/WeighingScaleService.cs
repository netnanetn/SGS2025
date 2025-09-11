using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Services
{
    public class WeighingScaleService
    {
        private readonly SerialPort _serialPort;
        private StringBuilder _buffer = new();
        private ScaleProtocol _protocol = ScaleProtocol.Unknown; // Unknown ban đầu
        private static string dataPlus = "";

        public bool IsConnected => _serialPort?.IsOpen ?? false;
        public event Action<double>? DataReceived;

        public WeighingScaleService()
        {
            _serialPort = new SerialPort
            {
                Encoding = Encoding.ASCII,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Connect(string portName, int baudRate = 9600)
        {
            if (IsConnected) return; // idempotent

            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.Open();

            _protocol = ScaleProtocol.Unknown; // reset về auto detect
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                _serialPort.Close();
                _buffer.Clear();
                dataPlus = "";
                _protocol = ScaleProtocol.Unknown;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string chunk = _serialPort.ReadExisting();
                if (string.IsNullOrEmpty(chunk)) return;

                _buffer.Append(chunk);

                // Auto detect nếu chưa xác định protocol
                var weights = ScaleParser.ExtractWeights(ref _buffer, ref _protocol);

                foreach (var w in weights)
                    DataReceived?.Invoke(w);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COM error: {ex.Message}");
            }
        }
    }
}
