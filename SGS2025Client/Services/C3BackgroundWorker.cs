using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Services
{
    public class C3BackgroundWorker : IAsyncDisposable
    {
        private readonly IC3Service _service;
        private readonly PeriodicTimer _healthTimer;
        private readonly ILogger<C3BackgroundWorker>? _logger;

        private string _ip = "";
        private int _port;
        private string _password = "";
        private bool _autoReconnect = true;
        private bool _isStarted;

        public event Action<string>? OnLog;
        public event Action<string, string, int>? OnCardRead;

        public C3BackgroundWorker(IC3Service service, ILogger<C3BackgroundWorker>? logger = null)
        {
            _service = service;
            _logger = logger;
            _healthTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));

            _service.OnStatusChanged += msg =>
            {
                OnLog?.Invoke(msg);
                _logger?.LogInformation(msg);
            };

            _service.OnCardRead += (t, c, r) =>
            {
                MainThread.BeginInvokeOnMainThread(() => OnCardRead?.Invoke(t, c, r));
            };
        }

        public async Task StartAsync(string ip, int port, string password, bool autoReconnect = true)
        {
            if (_isStarted) return;
            _isStarted = true;

            _ip = ip;
            _port = port;
            _password = password;
            _autoReconnect = autoReconnect;

            OnLog?.Invoke("🔌 Bắt đầu worker PLC C3...");

            _ = Task.Run(async () =>
            {
                while (await _healthTimer.WaitForNextTickAsync())
                {
                    try
                    {
                        if (!_service.IsConnected || !await _service.CheckConnectionAsync())
                        {
                            OnLog?.Invoke("⚠️ Mất kết nối PLC C3, thử reconnect...");
                            bool ok = await _service.ConnectAsync(_ip, _port, _password);
                            if (!ok)
                                OnLog?.Invoke("❌ Reconnect thất bại, chờ lần sau...");
                        }
                        else
                        {
                            OnLog?.Invoke("✅ Kết nối PLC C3 ổn định");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Lỗi health check PLC C3");
                        OnLog?.Invoke($"💥 Lỗi health check: {ex.Message}");
                    }
                }
            });
        }

        public async Task StopAsync()
        {
            _isStarted = false;
            OnLog?.Invoke("🛑 Dừng worker PLC C3...");
            await _service.DisconnectAsync();
        }

        public async Task OpenDoorAsync(int doorId)
        {
            if (!_service.IsConnected)
            {
                OnLog?.Invoke("⚠️ Không thể mở cửa — chưa kết nối PLC C3");
                return;
            }

            OnLog?.Invoke($"🚪 Gửi lệnh mở cửa #{doorId}");
            await _service.OpenDoorAsync(doorId);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            _healthTimer.Dispose();
            _service.Dispose();
        }
    }
}
