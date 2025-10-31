
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Services
{
    public interface IM31Service : IAsyncDisposable
    {
        event Action<string>? OnStatusChanged;
        event Action<int, bool>? OnSensorChanged;

        bool IsConnected { get; }

        Task<bool> ConnectAsync(string ip, int port = 502);
        Task DisconnectAsync();

        Task<bool[]> ReadDIAsync(int start, int count);
        Task<bool[]> ReadDOAsync(int start, int count);
        Task WriteDOAsync(int index, bool state);
    }

    public class M31Service : IM31Service
    {
        private TcpClient? _client;
        private IModbusMaster? _master;
        private CancellationTokenSource? _cts;
        private readonly SemaphoreSlim _modbusLock = new(1, 1);

        private bool[] _lastDI = Array.Empty<bool>();

        public event Action<string>? OnStatusChanged;
        public event Action<int, bool>? OnSensorChanged;

        public bool IsConnected => _client?.Connected ?? false;

        private string _ip = "";
        private int _port = 502;
        private readonly int _pollCount = 8;

        public async Task<bool> ConnectAsync(string ip, int port = 502)
        {
            _ip = ip;
            _port = port;

            try
            {
                await DisconnectAsync().ConfigureAwait(false); // clean previous

                _client = new TcpClient();
                _client.ReceiveTimeout = 1000;
                _client.SendTimeout = 1000;
                await _client.ConnectAsync(_ip, _port).ConfigureAwait(false);
                  

                // Dùng API cổ (đơn giản, phổ biến)
                _master = ModbusIpMaster.CreateIp(_client);
                _master.Transport.ReadTimeout = 2000;
                _master.Transport.WriteTimeout = 2000;

                OnStatusChanged?.Invoke($"✅ Đã kết nối {_ip}:{_port}");

                // start polling loop
                _cts = new CancellationTokenSource();
                _lastDI = new bool[_pollCount];
                 _ = Task.Run(() => PollLoopAsync(_cts.Token));

                return true;
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"❌ Kết nối thất bại: {ex.Message}");
                await DisconnectAsync().ConfigureAwait(false);
                return false;
            }
        }

        private async Task PollLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_master == null || !_client!.Connected)
                    {
                        OnStatusChanged?.Invoke("⚠️ Poll loop: chưa có kết nối");
                        await Task.Delay(1000, token).ConfigureAwait(false);
                        continue;
                    }

                    // read DI and DO sequentially under lock to avoid concurrent access issues
                    bool[] di = await ReadDIAsync(0, _pollCount).ConfigureAwait(false);
                    bool[] dos = await ReadDOAsync(0, _pollCount).ConfigureAwait(false);

                    // raise sensor change events for DI
                    for (int i = 0; i < di.Length; i++)
                    {
                        if (i >= _lastDI.Length) break;
                        if (di[i] != _lastDI[i])
                        {
                            _lastDI[i] = di[i];
                            // marshal to UI thread if needed
                            MainThread.BeginInvokeOnMainThread(() => OnSensorChanged?.Invoke(i, di[i]));
                        }
                    }

                    // short delay
                    await Task.Delay(1000, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnStatusChanged?.Invoke($"⚠️ Poll loop lỗi: {ex.Message}");
                    // attempt to reconnect
                    await SafeReconnectAsync(token).ConfigureAwait(false);
                }
            }
        }

        private async Task SafeReconnectAsync(CancellationToken token)
        {
            try
            {
                await DisconnectAsync().ConfigureAwait(false);
                // simple reconnect loop, try a few times until token cancelled
                while (!token.IsCancellationRequested)
                {
                    OnStatusChanged?.Invoke("🔄 Thử reconnect M31...");
                    bool ok = await ConnectAsync(_ip, _port).ConfigureAwait(false);
                    if (ok) return;
                    await Task.Delay(5000, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"❌ Reconnect error: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _cts?.Cancel();
                await Task.Delay(50).ConfigureAwait(false);

                if (_client != null)
                {
                    try
                    {
                        _client.Close();
                    }
                    catch { }
                    try
                    {
                        _client.Dispose();
                    }
                    catch { }
                    _client = null;
                }

                _master = null;
                OnStatusChanged?.Invoke("🔌 Đã ngắt kết nối M31");
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"⚠️ Lỗi khi ngắt kết nối: {ex.Message}");
            }
        }

        public async Task<bool[]> ReadDIAsync(int start, int count)
        {
            if (_master == null) return new bool[count];

            await _modbusLock.WaitAsync().ConfigureAwait(false);
            try
            {
                // NModbus ReadInputs (discrete inputs)
                var res = await Task.Run(() => _master.ReadInputs(1, (ushort)start, (ushort)count)).ConfigureAwait(false);
                return res ?? new bool[count];
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"⚠️ Lỗi đọc DI: {ex.Message}");
                return new bool[count];
            }
            finally
            {
                _modbusLock.Release();
            }
        }

        public async Task<bool[]> ReadDOAsync(int start, int count)
        {
            if (_master == null) return new bool[count];

            await _modbusLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var res = await Task.Run(() => _master.ReadCoils(1,(ushort)start, (ushort)count)).ConfigureAwait(false);
                return res ?? new bool[count];
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"⚠️ Lỗi đọc DO: {ex.Message}");
                return new bool[count];
            }
            finally
            {
                _modbusLock.Release();
            }
        }
         
        public async Task WriteDOAsync(int index, bool state)
        {
            if (_master == null || !_client.Connected)
                throw new InvalidOperationException("Chưa kết nối PLC M31");

            await _modbusLock.WaitAsync().ConfigureAwait(false);
            try
            {
                OnStatusChanged?.Invoke($"➡️ Ghi DO{index}...");
                await Task.Run(() =>
                {
                    lock (_master)
                    {
                        _master.WriteSingleCoil(1, (ushort)index, state);
                    }
                }).ConfigureAwait(false);

                OnStatusChanged?.Invoke($"✅ DO{index} = {(state ? "ON" : "OFF")}");
            }
            catch (IOException ex)
            {
                OnStatusChanged?.Invoke($"⚠️ Lỗi IO khi ghi DO: {ex.Message}");
                _ = SafeReconnectAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke($"⚠️ Lỗi ghi DO: {ex.Message}");
            }
            finally
            {
                if (_modbusLock.CurrentCount == 0)
                    _modbusLock.Release();
            }
        }

        // Proper async dispose
        public async ValueTask DisposeAsync()
        {
            // cancel polling
            try
            {
                _cts?.Cancel();
            }
            catch { }

            // small wait for loops to notice cancellation
            await Task.Delay(50).ConfigureAwait(false);

            try
            {
                if (_client != null)
                {
                    try { _client.Close(); } catch { }
                    try { _client.Dispose(); } catch { }
                    _client = null;
                }
            }
            catch { }

            try
            {
                _master = null;
            }
            catch { }

            try
            {
                _cts?.Dispose();
                _cts = null;
            }
            catch { }

            try
            {
                _modbusLock?.Dispose();
            }
            catch { }

            OnStatusChanged = null;
            OnSensorChanged = null;

            GC.SuppressFinalize(this);
        }
    }
}
