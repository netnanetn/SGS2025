using Azure.Core;
using CMS_Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Services
{
    public class AuthOnlineService : IAuthOnlineService
    {
        private readonly HttpClient _client;

        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }

        public AuthOnlineService(HttpClient client)
        {
            _client = client;

            // Load token lưu sẵn nếu có
            AccessToken = Preferences.Get(nameof(AccessToken), null);
            RefreshToken = Preferences.Get(nameof(RefreshToken), null);
        }

        // === LOGIN ===
        public async Task<bool> LoginAsync(string username, string password)
        {
            var payload = new
            {
                username,
                password
            };

            var response = await _client.PostAsJsonAsync("Account/Login", payload);
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null || result.errorCode != "00")
                return false;

            // Lưu token vào bộ nhớ & Preferences
            await SaveTokensAsync(result.access_token, result.refreshToken);
            return true;
        }

        // === LƯU TOKEN ===
        public async Task SaveTokensAsync(string access, string refresh)
        {
            AccessToken = access;
            RefreshToken = refresh;

            Preferences.Set(nameof(AccessToken), access);
            Preferences.Set(nameof(RefreshToken), refresh);
        }

        public async Task<string?> GetAccessTokenAsync() => AccessToken;
        public async Task<string?> GetRefreshTokenAsync() => RefreshToken;

        // === REFRESH TOKEN ===
        public async Task<bool> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(RefreshToken))
                return false;

            var response = await _client.PostAsJsonAsync("Account/RefreshToken", new { accessToken = AccessToken, refreshToken = RefreshToken });
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null && !string.IsNullOrEmpty(result.access_token))
            {
                await SaveTokensAsync(result.access_token, result.refreshToken);
                return true;
            }
            else
            {
                AccessToken = null;
                RefreshToken = null;
                Preferences.Remove(nameof(AccessToken));
                Preferences.Remove(nameof(RefreshToken));
            }

            return false;
        }

        // === ĐĂNG XUẤT ===
        public void Logout()
        {
            AccessToken = null;
            RefreshToken = null;
            Preferences.Remove(nameof(AccessToken));
            Preferences.Remove(nameof(RefreshToken));
        }

        // ==== Model JSON mapping ====

        private class LoginResponse
        {
            public string[]? listRole { get; set; }
            public string access_token { get; set; } = "";
            public int expires_in { get; set; }
            public string refreshToken { get; set; } = "";
           // public int refreshTokenExpiryTime { get; set; }
            public string errorCode { get; set; } = "";
            public string message { get; set; } = "";
            public string userId { get; set; } = "";
        }
         
    }
}
