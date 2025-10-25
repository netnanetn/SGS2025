using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // để dùng MainThread
using Windows.Storage.Streams;

namespace SGS2025Client.Services
{
    public interface IC3Service : IDisposable
    {
        bool IsConnected { get; }
        event Action<string>? OnStatusChanged;
        event Action<string, string, int>? OnCardRead;

        Task<bool> ConnectAsync(string ip, int port, string password);
        Task DisconnectAsync();
        Task OpenDoorAsync(int doorId, int delay = 3);
        Task<bool> CheckConnectionAsync();
    }

    public class C3Service : IC3Service
    {
        private IntPtr _hConn = IntPtr.Zero;
        private bool _isListening;
        private Thread? _listenThread;

        private string _ip = "";
        private int _port;
        private string _password = "";

        public event Action<string>? OnStatusChanged;
        public event Action<string, string, int>? OnCardRead;

        public bool IsConnected => _hConn != IntPtr.Zero;

        public async Task<bool> ConnectAsync(string ip, int port, string password)
        {
            _ip = ip;
            _port = port;
            _password = password;

            return await Task.Run(() =>
            {
                string connStr = $"protocol=TCP,ipaddress={ip},port={port},timeout=2000,passwd={password}";
                _hConn = C3SDK.Connect(connStr);

                if (_hConn != IntPtr.Zero)
                {
                    _isListening = true;
                    _listenThread = new Thread(() => ListenRealtime(_hConn))
                    {
                        IsBackground = true
                    };
                    _listenThread.Start();

                    OnStatusChanged?.Invoke("✅ Đã kết nối tới PLC C3-400");
                    return true;
                }

                OnStatusChanged?.Invoke("❌ Kết nối thất bại. Kiểm tra IP/Port/Mật khẩu");
                return false;
            });
        }

        private void ListenRealtime(IntPtr hConn)
        {
            byte[] buffer = new byte[256];

            while (_isListening)
            {
                try
                {
                    int ret = C3SDK.GetRTLog(hConn, ref buffer[0], buffer.Length);

                    if (ret < 0)
                    {
                        // lỗi đọc log → có thể bị ngắt mạng
                        OnStatusChanged?.Invoke("⚠️ Mất kết nối tới PLC C3 (GetRTLog lỗi)");
                        _ = DisconnectAsync(); // ngắt ngay
                        break;
                    }

                    if (ret > 0)
                    {
                        string data = Encoding.Default.GetString(buffer);
                        string[] tmp = data.Split(',');

                        if (tmp.Length > 3 && tmp[2] != "0")
                        {
                            string time = tmp[0];
                            string card = tmp[2];
                            int reader = int.Parse(tmp[3]);

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                OnCardRead?.Invoke(time, card, reader);
                            });
                        }
                    }

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    OnStatusChanged?.Invoke($"💥 Lỗi listener PLC C3: {ex.Message}");
                    _ = DisconnectAsync();
                    break;
                }
            }
        }

        public async Task<bool> CheckConnectionAsync()
        {
            if (_hConn == IntPtr.Zero)
                return false;

            try
            {
                // 1️⃣ Thử ping IP thật sự
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(_ip, 1000);
                if (reply.Status != IPStatus.Success)
                    return false;

                // 2️⃣ Gửi lệnh nhẹ
                byte[] buffer = new byte[128];
                int ret = C3SDK.GetRTLog(_hConn, ref buffer[0], buffer.Length);
                return ret >= 0;
            }
            catch
            {
                return false;
            }
        }

        public Task DisconnectAsync()
        {
            return Task.Run(() =>
            {
                _isListening = false;
                if (_hConn != IntPtr.Zero)
                {
                    C3SDK.Disconnect(_hConn);
                    _hConn = IntPtr.Zero;
                    OnStatusChanged?.Invoke("🔌 Đã ngắt kết nối PLC C3");
                }
            });
        }

        public Task OpenDoorAsync(int doorId, int delay = 3)
        {
            return Task.Run(() =>
            {
                if (_hConn == IntPtr.Zero) return;
                C3SDK.ControlDevice(_hConn, 1, doorId, 1, 1, delay, "");
            });
        }

        public void Dispose()
        {
            _isListening = false;
            if (_hConn != IntPtr.Zero)
            {
                C3SDK.Disconnect(_hConn);
                _hConn = IntPtr.Zero;
            }
        }

        private static class C3SDK
        {
            [DllImport("plcommpro.dll", EntryPoint = "Connect", CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr Connect(string connStr);

            [DllImport("plcommpro.dll", EntryPoint = "Disconnect", CallingConvention = CallingConvention.StdCall)]
            public static extern void Disconnect(IntPtr hConn);

            [DllImport("plcommpro.dll", EntryPoint = "GetRTLog", CallingConvention = CallingConvention.StdCall)]
            public static extern int GetRTLog(IntPtr hConn, ref byte buffer, int bufferSize);

            [DllImport("plcommpro.dll", EntryPoint = "ControlDevice", CallingConvention = CallingConvention.StdCall)]
            public static extern int ControlDevice(IntPtr hConn, int operationId, int doorId, int outputType, int doorState, int delay, string remark);
        }
    }
}
