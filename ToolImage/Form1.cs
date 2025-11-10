using System.Text.Json;

namespace ToolImage
{
    public partial class Form1 : Form
    {
        private Point startPoint;
        private Point endPoint;
        private bool isDrawing = false;
        private Image? originalImage;

        private PictureBox pictureBoxMain;
        private PictureBox pictureBoxPreview;
        private Label lblCoords;
        private TextBox txtJson;
        private Button btnCopy;

        public Form1()
        {
            InitializeComponent();
            this.Text = "🧭 OCR Coordinate Picker";
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;

            // === PictureBox chính ===
            pictureBoxMain = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 800,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };
            this.Controls.Add(pictureBoxMain);

            // === Panel bên phải ===
            var panelRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            this.Controls.Add(panelRight);

            // === Nút chọn ảnh ===
            var btnOpen = new Button
            {
                Text = "📂 Chọn ảnh...",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnOpen.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog();
                ofd.Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    originalImage = Image.FromFile(ofd.FileName);
                    pictureBoxMain.Image = (Image)originalImage.Clone();
                }
            };
            panelRight.Controls.Add(btnOpen);

            // === Label tọa độ ===
            lblCoords = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Consolas", 10, FontStyle.Bold),
                Text = "X: 0, Y: 0"
            };
            panelRight.Controls.Add(lblCoords);

            // === PictureBox preview vùng crop ===
            pictureBoxPreview = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 250,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            panelRight.Controls.Add(pictureBoxPreview);

            // === TextBox JSON ===
            txtJson = new TextBox
            {
                Dock = DockStyle.Top,
                Multiline = true,
                Height = 100,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };
            panelRight.Controls.Add(txtJson);

            // === Copy Button ===
            btnCopy = new Button
            {
                Text = "📋 Copy JSON",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnCopy.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtJson.Text))
                {
                    Clipboard.SetText(txtJson.Text);
                    MessageBox.Show("✅ Đã copy JSON vào clipboard!", "Thành công");
                }
            };
            panelRight.Controls.Add(btnCopy);

            // === Sự kiện chuột ===
            pictureBoxMain.MouseDown += PictureBoxMain_MouseDown;
            pictureBoxMain.MouseMove += PictureBoxMain_MouseMove;
            pictureBoxMain.MouseUp += PictureBoxMain_MouseUp;
        }

        private void PictureBoxMain_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && originalImage != null)
            {
                isDrawing = true;
                startPoint = e.Location;
            }
        }

        private void PictureBoxMain_MouseMove(object? sender, MouseEventArgs e)
        {
            lblCoords.Text = $"X: {e.X}, Y: {e.Y}";
            if (isDrawing && originalImage != null)
            {
                pictureBoxMain.Image?.Dispose();
                var temp = (Image)originalImage.Clone();
                using var g = Graphics.FromImage(temp);
                endPoint = e.Location;
                var rect = GetRectangle(startPoint, endPoint);
                g.DrawRectangle(new Pen(Color.Red, 2), rect);
                pictureBoxMain.Image = temp;
            }
        }

        private void PictureBoxMain_MouseUp(object? sender, MouseEventArgs e)
        {
            if (isDrawing && originalImage != null)
            {
                isDrawing = false;
                endPoint = e.Location;
                var rectOnControl = GetRectangle(startPoint, endPoint);

                // Chuyển tọa độ trong PictureBox sang tọa độ thật của ảnh
                var rectInImage = ConvertToImageCoordinates(rectOnControl, pictureBoxMain, originalImage);

                // Crop ảnh preview
                using var bmp = new Bitmap(originalImage);
                if (rectInImage.Width > 0 && rectInImage.Height > 0)
                {
                    var cropped = bmp.Clone(rectInImage, bmp.PixelFormat);
                    pictureBoxPreview.Image?.Dispose();
                    pictureBoxPreview.Image = cropped;

                    var json = JsonSerializer.Serialize(new
                    {
                        x = rectInImage.X,
                        y = rectInImage.Y,
                        width = rectInImage.Width,
                        height = rectInImage.Height
                    }, new JsonSerializerOptions { WriteIndented = true });

                    txtJson.Text = json;
                }
            }
        }

        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y));
        }

        private Rectangle ConvertToImageCoordinates(Rectangle rect, PictureBox pb, Image img)
        {
            if (img == null) return rect;

            // Tính tỉ lệ giữa ảnh hiển thị và ảnh gốc
            float imageRatio = (float)img.Width / img.Height;
            float boxRatio = (float)pb.Width / pb.Height;

            int viewWidth, viewHeight;
            if (imageRatio > boxRatio)
            {
                viewWidth = pb.Width;
                viewHeight = (int)(pb.Width / imageRatio);
            }
            else
            {
                viewHeight = pb.Height;
                viewWidth = (int)(pb.Height * imageRatio);
            }

            int offsetX = (pb.Width - viewWidth) / 2;
            int offsetY = (pb.Height - viewHeight) / 2;

            float scaleX = (float)img.Width / viewWidth;
            float scaleY = (float)img.Height / viewHeight;

            int x = (int)((rect.X - offsetX) * scaleX);
            int y = (int)((rect.Y - offsetY) * scaleY);
            int w = (int)(rect.Width * scaleX);
            int h = (int)(rect.Height * scaleY);

            return new Rectangle(x, y, w, h);
        }
    }
}
