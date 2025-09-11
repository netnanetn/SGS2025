using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Services
{
    public class WeighingScaleService
    {
        private SerialPort _serialPort;
        private readonly ScaleProtocol _protocol;
        private StringBuilder _buffer = new StringBuilder();

        public event Action<double> DataReceived;

        public WeighingScaleService(ScaleProtocol protocol)
        {
            _protocol = protocol;
        }
        public bool IsConnected => _serialPort?.IsOpen ?? false;
        public void Connect(string portName = "COM3", int baudRate = 9600)
        {
            // tránh connect nhiều lần
            if (IsConnected) return;

            _serialPort = new SerialPort(portName, baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                Encoding = System.Text.Encoding.ASCII,
                NewLine = "\r\n",
                ReadTimeout = 200
            };

            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string chunk = _serialPort.ReadExisting();
                if (string.IsNullOrEmpty(chunk)) return;

                _buffer.Append(chunk);

                var weights = ScaleParser.ExtractWeights(ref _buffer, _protocol);

                foreach (var w in weights)
                    DataReceived?.Invoke(w);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"COM error: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.DataReceived -= SerialPort_DataReceived;
                        _serialPort.Close();
                    }
                }
                catch { /* swallow or log */ }
                finally
                {
                    _serialPort?.Dispose();
                    _serialPort = null;
                }
            }
        }
    }
}
