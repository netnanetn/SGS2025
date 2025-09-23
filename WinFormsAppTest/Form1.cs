using System.Net.WebSockets;
using System.Text;

namespace WinFormsAppTest
{
    public partial class Form1 : Form
    {
        private ClientWebSocket _ws;
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            _ws = new ClientWebSocket();
            try
            {
                await _ws.ConnectAsync(new Uri("ws://localhost:8000"), CancellationToken.None);
                Log("✅ Kết nối thành công!");

                // Bắt đầu lắng nghe message
                _ = Task.Run(ReceiveLoop);
            }                                                                                                                                                                                                                                                                                                                                       
            catch (Exception ex)
            {
                Log("❌ Lỗi: " + ex.Message);
            }
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];

            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        Log("🔌 Server đóng kết nối.");
                    }
                    else
                    {
                        string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Log("⬅️ Received: " + msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("❌ Receive error: " + ex.Message);
            }
        }

        private void Log(string text)
        {
            if (txtDataLog.InvokeRequired)
            {
                txtDataLog.Invoke(new Action(() => txtDataLog.AppendText(text + Environment.NewLine)));
            }
            else
            {
                txtDataLog.AppendText(text + Environment.NewLine);
            }
        }

        private async  void btnSend_Click(object sender, EventArgs e)
        {
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                string message = "Hello from WinForms!";
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                Log("➡️ Sent: " + message);
            }
        }
    }
}
