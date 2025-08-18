using DahuaNativeViewer;

namespace DahuaUICamera
{
    public partial class Form1 : Form
    {
        private DahuaPlayerControl _player = new() { Dock = DockStyle.Fill };
        private Button _btnStart = new() { Text = "Start", Dock = DockStyle.Top, Height = 40 };
        private Button _btnCap = new() { Text = "Capture", Dock = DockStyle.Top, Height = 40 };


        public Form1()
        {
            InitializeComponent();
            Text = "Dahua Native Viewer";
            Controls.Add(_player);
            Controls.Add(_btnCap);
            Controls.Add(_btnStart);

            _btnStart.Click += (_, __) => _player.Start();
            _btnCap.Click += (_, __) =>
            {
                var path = System.IO.Path.Combine(AppContext.BaseDirectory,
                    $"cap_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                if (_player.CaptureJpeg()) MessageBox.Show("Saved: " + path);
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
