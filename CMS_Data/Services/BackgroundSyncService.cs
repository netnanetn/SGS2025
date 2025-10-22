using CMS_Data.Enums;
using CMS_Data.Interfaces;
using CMS_Data.Models;
using CMS_Data.Networks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class BackgroundSyncService
    {
        private readonly INetworkStatusProvider _network;
        private readonly IDbContextFactory<MoDaContext> _factory;
        private readonly IApiService _api;
        private System.Threading.Timer _timer;
        private bool _isSyncing;
        private ScaleService _scaleService;
        private readonly IAuthOnlineService _auth;
        private readonly IConfigService _config;

        public BackgroundSyncService(INetworkStatusProvider network, IDbContextFactory<MoDaContext> factory, IApiService api, ScaleService scaleService, IAuthOnlineService auth, IConfigService config)
        {
            _network = network;
            _factory = factory;
            _api = api;
            _scaleService = scaleService;
            _auth = auth;
            _config = config;
        }
        // --- Auto sync định kỳ ---
        public void StartAutoSync(int timeLoop = 60)
        { 
            _timer = new System.Threading.Timer(
                async _ => await RunSyncAsync(),
                null,
                TimeSpan.Zero,            // chạy ngay
                TimeSpan.FromSeconds(timeLoop) // lặp lại mỗi 5 phút
            );
        }
        public void StopAutoSync()
        {
            _timer?.Dispose();
            _timer = null;
        }

        // 🔄 Đồng bộ thủ công ngay lập tức
        public async Task ForceSyncNow()
        {
            await RunSyncAsync();
        }
        private async Task RunSyncAsync()
        {

            var cfg = await _config.LoadAsync();
            if (!cfg.Sync.AllowSyncOnline)
                return; // offline mode, không đồng bộ

            if (_isSyncing) return; // tránh chạy trùng
            _isSyncing = true;

            try
            {
                if (!_network.IsConnected)
                {
                    Console.WriteLine("⚠️ Không có mạng, tạm ngưng đồng bộ.");
                    return;
                }
                var unsynced = await _scaleService.GetUnsyncedAsync(20);
                foreach (var rec in unsynced)
                {
                    try
                    {
                        if (!_network.IsConnected)
                        {
                            Console.WriteLine("⚠️ Mất mạng giữa chừng, dừng đồng bộ.");
                            break;
                        }

                        rec.SyncFailCount++;

                        var (success, serverId, error) = await _api.SendRecordAsync(rec);
                        if (success)
                        {
                            rec.SyncStatus = (int)SyncStatus.Synced; // đã đồng bộ
                            rec.SyncTime = DateTime.Now;
                            rec.ServerId = serverId;
                            rec.SyncError = null;
                            rec.SyncFailCount = 0;
                        }
                        else
                        {
                            rec.SyncFailCount++;

                            if (rec.SyncFailCount >= 5)
                            {
                                rec.SyncStatus = (int)SyncStatus.Failed; // lỗi vĩnh viễn, ngưng retry
                                rec.SyncError = error;
                            }
                            else
                            {
                                rec.SyncStatus = (int)SyncStatus.NotSynced; ; // vẫn để ở trạng thái chờ để retry lần sau
                                rec.SyncError = error;
                            }
                        }

                        await _scaleService.UpdateScaleAll(rec);
                        await Task.Delay(1000);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
