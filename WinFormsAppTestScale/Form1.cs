namespace WinFormsAppTestScale
{
    public partial class Form1 : Form
    {
        private readonly WeighingScaleService _scaleService;
        private readonly WeighingScaleTcpService _tcpService = new();
        private readonly bool usingTCPIP = false;
        public Form1()
        {
            InitializeComponent();
            _scaleService = new WeighingScaleService();
            _scaleService.DataReceived += OnWeightReceived;
            _scaleService.RawReceived += OnRawReceived;
        }

        private async void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (usingTCPIP) {
                    _tcpService.DataReceived += OnWeightReceived;
                    _tcpService.RawReceived += OnRawReceived;
                    await _tcpService.StartAsync(31000);
                }
                else {
                    var comport = this.txtCOM.Text;
                    var baud = Int32.Parse(this.txtBaudrate.Text);
                    _scaleService.Connect(comport, baud);
                    MessageBox.Show("Đã kết nối cân.");
                }

               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (usingTCPIP)
            {
                _tcpService.Stop();
            }
            else
            {
                _scaleService.Disconnect();
                MessageBox.Show("Đã ngắt kết nối.");
            }
           
        }
        private void OnWeightReceived(double weight)
        {
            // cần Invoke để update UI thread
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => lbWeight.Text = $"{weight:0.###} kg"));
            }
            else
            {
                lbWeight.Text = $"{weight:0.###} kg";
            }
        }
        private void OnRawReceived(string raw)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => rtxtLog.AppendText(raw)));
            }
            else
            {
                rtxtLog.AppendText(raw);
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            rtxtLog.Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
