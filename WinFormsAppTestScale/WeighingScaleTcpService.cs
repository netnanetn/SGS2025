using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsAppTestScale
{
    public class WeighingScaleTcpService
    {
        private TcpListener _listener;
        private bool _running;
        private ScaleProtocol _protocol = ScaleProtocol.Unknown;
        private StringBuilder _buffer = new();

        public event Action<double>? DataReceived;
        public event Action<string>? RawReceived;

        public async Task StartAsync(int port = 31000)
        {
            if (_running) return;

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _running = true;

            Console.WriteLine($"Listening on port {port}...");

            while (_running)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine("Client connected");
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (_running && client.Connected)
            {
                int bytesRead = 0;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch
                {
                    break; // lỗi đọc → ngắt kết nối
                }

                if (bytesRead <= 0) break;

                string chunk = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // bắn log raw
                RawReceived?.Invoke(chunk);

                _buffer.Append(chunk);

                var weights = ScaleParser.ExtractWeights(ref _buffer, ref _protocol);
                foreach (var w in weights)
                    DataReceived?.Invoke(w);
            }

            client.Close();
            Console.WriteLine("Client disconnected");
        }
    }
}
