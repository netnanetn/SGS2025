using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025.Core.Services.ShareServices
{
    public class ImageStorageService
    {
        private readonly string _rootPath = @"C:\TVS\Images";

        public ImageStorageService()
        {
            Directory.CreateDirectory(_rootPath);
        }

        public async Task<string?> SaveBase64Async(string base64, string phieuCode, string prefix = "")
        {
            if (string.IsNullOrEmpty(base64)) return null;
            var cleanBase64 = base64.Contains(",")
            ? base64.Substring(base64.IndexOf(",") + 1)
            : base64;

            var bytes = Convert.FromBase64String(cleanBase64);
             

            // Tổ chức theo ngày
            var dayFolder = Path.Combine(_rootPath, DateTime.Now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(dayFolder);

            var fileName = $"{phieuCode}_{prefix}_{DateTime.Now:HHmmss}.jpg";
            var filePath = Path.Combine(dayFolder, fileName);

            await File.WriteAllBytesAsync(filePath, bytes);

            // Trả về URL cho BlazorWebView
            // => https://local.tvs/yyyy-MM-dd/abc_Lan1_Cam1.jpg
           // return $"https://local.tvs/{DateTime.Now:yyyy-MM-dd}/{fileName}";
            return filePath; // Trả về đường dẫn để lưu DB
        }
        public string ConvertImageToBase64(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                var bytes = File.ReadAllBytes(filePath);
                var base64 = Convert.ToBase64String(bytes);
                return $"data:image/jpeg;base64,{base64}";
            }
            catch
            {
                return string.Empty;
            }
        }
    }

}
