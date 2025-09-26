using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace ScaleKey
{
    record LicenseInfo(string Customer, string HardwareId, DateTime ExpireDate, string Signature);
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCreateKey_Click(object sender, EventArgs e)
        {
            var customer = this.txtCustomer.Text;
            var hardwareId = this.txtHardwareId.Text;
            var day = int.Parse(this.txtDay.Text);
            if (customer == null || hardwareId == null || day < 1) {
                MessageBox.Show("Không hợp lệ","Thông báo", MessageBoxButtons.OK) ;
                return;
            }
            // 1) Nếu chưa có key, sinh key
            if (!File.Exists("private.pem") || !File.Exists("public.pem"))
            {
                using var rsa = RSA.Create(2048);
                var privatePem = rsa.ExportRSAPrivateKeyPem();
                var publicPem = rsa.ExportRSAPublicKeyPem(); // hoặc ExportSubjectPublicKeyInfoPem
                File.WriteAllText("private.pem", privatePem);
                File.WriteAllText("public.pem", publicPem);
                Console.WriteLine("Generated private.pem & public.pem");
            }

            // 2) Tạo license mẫu (thay bằng tham số khi dùng thực tế)
            var license = new
            {
                Customer = customer,
                HardwareId = hardwareId,
                ExpireDate = DateTime.UtcNow.AddDays(day)
            };

            var data = $"{license.Customer}|{license.HardwareId}|{license.ExpireDate:yyyyMMdd}";
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var rsa2 = RSA.Create();
            rsa2.ImportFromPem(File.ReadAllText("private.pem"));

            var sig = rsa2.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var signatureBase64 = Convert.ToBase64String(sig);

            var outLicense = new
            {
                Customer = license.Customer,
                HardwareId = license.HardwareId,
                ExpireDate = license.ExpireDate,
                Signature = signatureBase64
            };

            var json = JsonSerializer.Serialize(outLicense, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText($"license-{license.Customer}.lic", json);
            MessageBox.Show("Cấp key thành công", "Thông báo", MessageBoxButtons.OK);
        }
    }
}
