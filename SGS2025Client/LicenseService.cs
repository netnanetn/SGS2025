using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Win32;

namespace SGS2025Client
{
    public class LicenseService
    {
        private string _publicPem;

        public LicenseService()
        {
            // đọc public.pem từ tài nguyên gói
            _publicPem = LoadPublicPemAsync().GetAwaiter().GetResult();
        }

        private async Task<string> LoadPublicPemAsync()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("SGS2025Client.Keys.public.pem");
                using var reader = new StreamReader(stream);
                string pem = reader.ReadToEnd();
                return pem;
            }
            catch
            {
                return null;
            }
        }
         
        public bool ValidateLicense()
        {
            return true;
            // 1) tìm license file: có thể ở AppDataDirectory/license.lic
            var licensePath = Path.Combine(FileSystem.AppDataDirectory, "license.lic");
            if (!File.Exists(licensePath))
            {
                return false;
            }

            var json = File.ReadAllText(licensePath);
            var license = JsonSerializer.Deserialize<LicenseInfo>(json);
            if (license == null) return false;

            // 2) verify signature
            var data = $"{license.Customer}|{license.HardwareId}|{license.ExpireDate:yyyyMMdd}";
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(_publicPem);

            var sig = Convert.FromBase64String(license.Signature);
            var validSig = rsa.VerifyData(dataBytes, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            if (!validSig) return false;

            // 3) check expiration
            if (license.ExpireDate < DateTime.UtcNow) return false;

            // 4) check hardware id
            var localHw = GetLocalHardwareId();
            if (!string.Equals(localHw, license.HardwareId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // 5) optional: detect rollback of system clock
            if (!CheckSystemClockRollback(license.ExpireDate))
                return false;

            return true;
        }
        private bool CheckSystemClockRollback(DateTime expireDate)
        {
            return true;
            // ví dụ: lưu lastChecked vào file
            var path = Path.Combine(FileSystem.AppDataDirectory, "lastCheck.txt");
            if (File.Exists(path))
            {
                var lastStr = File.ReadAllText(path);
                if (DateTime.TryParse(lastStr, out var last))
                {
                    if (DateTime.UtcNow < last) // clock rollback detected
                        return false;
                }
            }

            // cập nhật lại lastCheck
            File.WriteAllText(path, DateTime.UtcNow.ToString("O"));
            return true;
        }
        public string GetLocalHardwareId_BAK()
        {
        #if WINDOWS
                    var mac = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                          .FirstOrDefault(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)?.GetPhysicalAddress().ToString();
                    return mac ?? "UNKNOWN";
        #elif ANDROID
                return Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver,
                                                                 Android.Provider.Settings.Secure.AndroidId);
        #elif IOS
                return UIKit.UIDevice.CurrentDevice.IdentifierForVendor.AsString();
        #else
                return "UNKNOWN";
        #endif
        }
        public  string GetLocalHardwareId()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        object guid = key.GetValue("MachineGuid");
                        if (guid != null)
                        {
                            return guid.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading MachineGuid: " + ex.Message);
            }
            return "UNKNOWN";
        }

    }
    public class LicenseInfo
    {
        public string Customer { get; set; }
        public string HardwareId { get; set; }
        public DateTime ExpireDate { get; set; }
        public string Signature { get; set; }
    }
}
