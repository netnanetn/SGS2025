using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LicensePlateGrpcService;
using Grpc.Core;

namespace OcrGrpcClient
{
    public class OcrClientHelper
    {
        private static OcrService.OcrServiceClient? _client;
        private static GrpcChannel? _channel;
        private static NamedPipeClientStream? _pipeStream;

        private static async Task EnsureConnectedAsync()
        {
            if (_client != null)
                return;

            _pipeStream = new NamedPipeClientStream(".", "OcrPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
            await _pipeStream.ConnectAsync(5000);

            var handler = new SocketsHttpHandler
            {
                ConnectCallback = async (context, token) => _pipeStream
            };

            var httpClient = new HttpClient(handler);
            _channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = httpClient });
            _client = new OcrService.OcrServiceClient(_channel);

            Console.WriteLine("✅ Connected to OCR gRPC NamedPipe.");
        }
        public static async Task InitAsync()
        {
            var pipeStream = new NamedPipeClientStream(".", "OcrPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
            await pipeStream.ConnectAsync(5000); // timeout 5s

            var handler = new SocketsHttpHandler
            {
                ConnectCallback = async (context, token) => pipeStream
            };

            var httpClient = new HttpClient(handler);
            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpClient = httpClient });
            _client = new OcrService.OcrServiceClient(channel);

            Console.WriteLine("✅ Connected to OCR gRPC NamedPipe.");
        }

        /// <summary>
        /// Gửi ảnh base64 sang server OCR để nhận diện.
        /// </summary>
        public static async Task<string> RecognizeAsync(string base64, string companyCode = "DEFAULT",
            int x = 0, int y = 0, int width = 0, int height = 0)
        {
            try
            {
                await EnsureConnectedAsync();

                var reply = await _client!.RecognizeAsync(new OcrRequest
                {
                    Base64 = base64,
                    CompanyCode = companyCode,
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                });

                return reply.Text;
            }
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unavailable)
            {
                Console.WriteLine("⚠️ OCR connection lost, reconnecting...");
                await DisposeAsync();
                await EnsureConnectedAsync();

                // Gọi lại lần nữa sau khi reconnect
                var reply = await _client!.RecognizeAsync(new OcrRequest
                {
                    Base64 = base64,
                    CompanyCode = companyCode,
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                });
                return reply.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi OCR: {ex.Message}");
                return string.Empty;
            }
        }
        public static async Task DisposeAsync()
        {
            try
            {
                _channel?.Dispose();
                _pipeStream?.Dispose();
                _client = null;
                _channel = null;
                _pipeStream = null;
                await Task.Delay(200);
            }
            catch { }
        }
    }
}
