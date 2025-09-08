using Microsoft.Identity.Client;
using SGS2025Client.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SGS2025Client.Services
{
    public class ConfigService
    {
        private readonly string _filePath;
        public AppConfig _config { get; private set; } = new();

        public ConfigService(string filePath)
        {
            var folder = @"C:\TVS\Config";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "appsettings.sgs.json");
        }

        /// <summary>
        /// Load cấu hình từ file JSON
        /// </summary>
        public async Task<AppConfig> LoadAsync()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException("Không tìm thấy file cấu hình", _filePath);

            var json = await File.ReadAllTextAsync(_filePath);
            _config = JsonSerializer.Deserialize<AppConfig>(json)
                       ?? throw new InvalidOperationException("Không load được cấu hình");

            return _config;
        }

        /// <summary>
        /// Lưu cấu hình hiện tại ra file JSON
        /// </summary>
        public async Task SaveAsync()
        {
            if (_config == null)
                throw new InvalidOperationException("Chưa load cấu hình");

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_config, options);
            await File.WriteAllTextAsync(_filePath, json);
        }
        public async Task SaveAsync(AppConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        /// <summary>
        /// Lấy cấu hình hiện tại trong memory
        /// </summary>
        public AppConfig GetConfig()
        {
            if (_config == null)
                throw new InvalidOperationException("Chưa load cấu hình");
            return _config;
        }

        /// <summary>
        /// Update URL của camera theo Code
        /// </summary>
        public bool UpdateCameraUrl(string code, string newUrl)
        {
            if (_config == null)
                throw new InvalidOperationException("Chưa load cấu hình");

            var cam = _config.Camera.List.FirstOrDefault(c => c.Code == code);
            if (cam == null) return false;

            cam.Url = newUrl;
            return true;
        }

        /// <summary>
        /// Update Barrier theo index (0-based)
        /// </summary>
        public bool UpdateBarrier(int index, string newIp, int newPort)
        {
            if (_config == null)
                throw new InvalidOperationException("Chưa load cấu hình");

            if (index < 0 || index >= _config.Barrier.Count)
                return false;

            _config.Barrier[index].Ip = newIp;
            _config.Barrier[index].Port = newPort;
            return true;
        }

        /// <summary>
        /// Update toàn bộ ComConfig
        /// </summary>
        public bool UpdateComConfig(ComConfig newConfig)
        {
            if (_config == null)
                throw new InvalidOperationException("Chưa load cấu hình");

            if (newConfig == null) return false;

            _config.ComConfig = newConfig;
            return true;
        }
    }
}
