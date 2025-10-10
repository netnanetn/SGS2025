using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace WinFormsAppTest
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;
        private bool enableCoi = true;
        private bool enableDenxanh = true;
        private bool enableDendo = true;
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient("192.168.1.220", 502); // IP và port PLC
                stream = client.GetStream();
                MessageBox.Show("Connected OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connect error: " + ex.Message);
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

        private async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Modbus TCP Frame giống modpoll: 
                // Tx: 00 01 00 00 00 06 01 02 00 00 00 04
                // Giải thích: Transaction=0x0001, Protocol=0x0000, Length=0x0006, UnitId=1,
                // Function=0x02, StartAddr=0x0000, Quantity=0x0004

                byte[] frame = new byte[]
                 {
                    0x00, 0x22, // Transaction ID (tùy chọn, có thể thay đổi mỗi lần gửi)
                    0x00, 0x00, // Protocol ID
                    0x00, 0x06, // Length
                    0x04,       // Unit ID (ở đây là 4)
                    0x02,       // Function Code = Read Discrete Inputs
                    0x00, 0x00, // Starting Address = 0
                    0x00, 0x05  // Quantity = 2
                 };

                stream.Write(frame, 0, frame.Length);

                // Nhận phản hồi
                byte[] buffer = new byte[256];
                int read = stream.Read(buffer, 0, buffer.Length);

                // In hex ra log textbox
                string hex = BitConverter.ToString(buffer, 0, read).Replace("-", " ");
                Log("Rx: " + hex + Environment.NewLine);

                byte lastByte = buffer[read - 1];
                bool[] DI = M31DecodeDI(lastByte);
                this.Invoke((MethodInvoker)delegate
                {
                    lbSensor1.ForeColor = DI[0] ? Color.Green : Color.Black;
                    lbSensor2.ForeColor = DI[1] ? Color.Green : Color.Black;
                    lbSensor3.ForeColor = DI[2] ? Color.Green : Color.Black;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send error: " + ex.Message);
            }
        }
        public static bool[] M31DecodeDI(byte lastByte)
        {
            bool[] DI = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                DI[i] = (lastByte & (1 << i)) != 0;
            }
            return DI;
        }
        private void btnCoiHu_Click(object sender, EventArgs e)
        {
            try
            {
                // Modbus TCP Frame giống modpoll: 
                // Tx: 00 01 00 00 00 06 01 02 00 00 00 04
                // Giải thích: Transaction=0x0001, Protocol=0x0000, Length=0x0006, UnitId=1,
                // Function=0x02, StartAddr=0x0000, Quantity=0x0004

                byte[] frame = new byte[]
                {
                    0x00, 0x01, // Transaction ID
                    0x00, 0x00, // Protocol ID
                    0x00, 0x06, // Length
                    0x04,       // Unit ID
                    0x05,       // Function = Write Single Coil
                    0x00, 0x00, // Coil Address = 2
                    0xFF, 0x00  // Value = ON
                };

                stream.Write(frame, 0, frame.Length);

                // Nhận phản hồi
                byte[] buffer = new byte[256];
                int read = stream.Read(buffer, 0, buffer.Length);

                // In hex ra log textbox
                string hex = BitConverter.ToString(buffer, 0, read).Replace("-", " ");
                Log("Rx: " + hex + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send error: " + ex.Message);
            }
        }

        private void btnStopCoiHu_Click(object sender, EventArgs e)
        {
            try
            {
                // Modbus TCP Frame giống modpoll: 
                // Tx: 00 01 00 00 00 06 01 02 00 00 00 04
                // Giải thích: Transaction=0x0001, Protocol=0x0000, Length=0x0006, UnitId=1,
                // Function=0x02, StartAddr=0x0000, Quantity=0x0004

                byte[] frame = new byte[]
                {
                    0x00, 0x01, // Transaction ID
                    0x00, 0x00, // Protocol ID
                    0x00, 0x06, // Length
                    0x04,       // Unit ID
                    0x05,       // Function = Write Single Coil
                    0x00, 0x00, // Coil Address = 2
                    0x00, 0x00  // OFF
                };

                stream.Write(frame, 0, frame.Length);

                // Nhận phản hồi
                byte[] buffer = new byte[256];
                int read = stream.Read(buffer, 0, buffer.Length);

                // In hex ra log textbox
                string hex = BitConverter.ToString(buffer, 0, read).Replace("-", " ");
                Log("Rx: " + hex + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send error: " + ex.Message);
            }
        }

        private void btnCoi_Click(object sender, EventArgs e)
        {
            M31WriteSingleCoil(4, 0, enableCoi);
            enableCoi = !enableCoi;
        }

        private void btnDenxanh_Click(object sender, EventArgs e)
        {
            M31WriteSingleCoil(4, 1, enableDenxanh);
            enableDenxanh = !enableDenxanh;
        }

        private void btnDendo_Click(object sender, EventArgs e)
        {
            M31WriteSingleCoil(4, 2, enableDendo);
            enableDendo = !enableDendo;
        }
        private void M31WriteSingleCoil(byte unitId, ushort coilAddress, bool turnOn)
        {
            try
            {
                byte[] frame = new byte[]
                {
                0x00, 0x01,
                0x00, 0x00,
                0x00, 0x06,
                unitId,
                0x05,
            (byte)(coilAddress >> 8), (byte)(coilAddress & 0xFF),
            turnOn ? (byte)0xFF : (byte)0x00, 0x00
                };

                stream.Write(frame, 0, frame.Length);

                byte[] buffer = new byte[256];
                int read = stream.Read(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send error: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
