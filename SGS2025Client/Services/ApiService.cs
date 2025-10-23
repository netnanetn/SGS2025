using CMS_Data.Interfaces;
using CMS_Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly IAuthOnlineService _auth;
        private readonly IConfigService _config;

        public ApiService(HttpClient client, IAuthOnlineService auth, IConfigService config)
        {
            _client = client;
            _auth = auth;
            _config = config;
        }
        private async Task EnsureTokenAsync()
        {
            var token = await _auth.GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                // Nếu chưa có token -> login online ẩn
                var config = await _config.LoadAsync(); // đọc file cấu hình
                await _auth.LoginAsync(config.Sync.LoginUser, config.Sync.LoginPassword);
                token = await _auth.GetAccessTokenAsync();
            }

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        public async Task<(bool Success, int ServerId, string? Error)> SendRecordAsync(TblScale record)
        {
            try
            {
                var postModel = new
                {
                    customerId = record.CustomerId ?? 0,
                    customerName = record.CustomerName,
                    customerAddress = record.CustomerAddress,
                    customerPhone = record.CustomerPhone,

                    productId = record.ProductId ?? 0,
                    productName = record.ProductName,
                    productNumber = record.ProductNumber ?? 0,
                    productPrice = record.ProductPrice ?? 0,
                    productMoney = record.ProductMoney ?? 0,

                    driverName = record.DriverName,
                    vehicle = record.Vehicle,

                    timeIn = record.TimeIn,
                    weightIn = record.WeightIn ?? 0,
                    timeOut = record.TimeOut,
                    weightOut = record.WeightOut ?? 0,

                    proportion = record.Proportion ?? 0,
                    exchange = record.Exchange ?? 0,

                    note = record.Note,
                    typeId = record.TypeId ?? 0,
                    topSeal = record.TopSeal,

                    impurity = record.WeightImpurity.HasValue && record.WeightIn.HasValue && record.WeightIn > 0
                       ? Math.Round((double)(record.WeightImpurity / record.WeightIn * 100), 3)
                       : 0,
                    weightImpurity = record.WeightImpurity ?? 0
                };

                // Gắn token vào Header trước khi gửi
                await EnsureTokenAsync();
                var response = await _client.PostAsJsonAsync("Scale/Create", postModel);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    bool refreshed = await _auth.RefreshTokenAsync();
                    if (refreshed)
                    {
                        await EnsureTokenAsync();
                        response = await _client.PostAsJsonAsync("Scale/Create", postModel);
                    }
                    else
                    {
                        await EnsureTokenAsync();
                        response = await _client.PostAsJsonAsync("Scale/Create", postModel);
                        //return (false, 0, "Token expired and refresh failed");
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiSyncResult>();
                    return (true, result?.Id ?? 0, null);
                }

                return (false, 0, response.ReasonPhrase);
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }
    }
    public class ApiSyncResult
    {
        public int Id { get; set; }
    }
}
