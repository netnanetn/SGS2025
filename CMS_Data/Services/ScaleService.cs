using CMS_Data.Enums;
using CMS_Data.ModelDTO;
using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class ScaleService
    {
        private readonly IDbContextFactory<MoDaContext> _factory;
        public ScaleService(IDbContextFactory<MoDaContext> factory)
        {
            _factory = factory;
        }
        public async Task<TblScale> GetById(int id)
        {
            using var _db = _factory.CreateDbContext();
            var scale = await _db.TblScales.AsNoTracking().FirstOrDefaultAsync(x=>x.Id == id);
            
            return scale;
        }
        public async Task<List<TblScale>> SearchAsync(string? q, int take = 20, CancellationToken ct = default)
        {
            using var _db = _factory.CreateDbContext();
            var query = _db.TblScales.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.Code.Contains(q) ||
                    (x.CustomerName != null && x.CustomerName.Contains(q)) ||
                    (x.ProductName != null && x.ProductName.Contains(q)));
            }
            var res = await query
                .OrderByDescending(x => x.Id)
                .Take(take)
                .ToListAsync(ct);
            return res;
        }
        public async Task<List<TblScale>> SearchByDateAsync(string? q, DateTime? FromDate, DateTime? ToDate, int take = 20, CancellationToken ct = default)
        {
            FromDate = FromDate?.Date.AddDays(-1).AddTicks(-1) ?? DateTime.MaxValue;
            ToDate = ToDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;
            using var _db = _factory.CreateDbContext();
            var query = _db.TblScales.AsNoTracking().AsQueryable();
            query = query.Where(p =>
                    (!FromDate.HasValue || p.CreateDay >= FromDate) &&
                    (!ToDate.HasValue || p.CreateDay <= ToDate));
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.Code.Contains(q) ||
                    (x.CustomerName != null && x.CustomerName.Contains(q)) ||
                    (x.ProductName != null && x.ProductName.Contains(q)));
            }
            var res = await query
                .OrderByDescending(x => x.Id)
                .Take(take)
                .ToListAsync(ct);
            return res;
        }
        public async Task<List<TblScale>> SearchByFilterDTOAsync(ScaleFilterDTO filter, int take = 50, CancellationToken ct = default)
        {
            using var _db = _factory.CreateDbContext();
            var query = _db.TblScales.AsNoTracking().AsQueryable();

            // --- 1️⃣ Lọc theo ngày ---
            if (filter.TuNgay.HasValue)
            {
                var fromDate = filter.TuNgay.Value.Date; // bắt đầu từ 00:00
                query = query.Where(x => x.CreateDay >= fromDate);
            }

            if (filter.DenNgay.HasValue)
            {
                var toDate = filter.DenNgay.Value.Date.AddDays(1).AddTicks(-1); // hết 23:59:59
                query = query.Where(x => x.CreateDay <= toDate);
            }

            // --- 2️⃣ Lọc theo tình trạng cân ---
            if (filter.XeChoCanLan2)
            {
                // Xe đã có lần 1 nhưng chưa cân lần 2 (WeightOut chưa có)
                query = query.Where(x => x.WeightIn > 0 && (x.WeightOut == null || x.WeightOut == 0));
            }

            if (filter.XeDaCan)
            {
                // Xe đã hoàn thành cả 2 lần cân
                query = query.Where(x => x.WeightOut != null && x.WeightOut > 0);
            }

            // --- 3️⃣ Lọc theo kiểu cân ---
            if (!string.IsNullOrEmpty(filter.KieuCan) && filter.KieuCan != "Tất cả")
            {
                // giả sử TypeId hoặc TypeName (nếu có mapping khác thì sửa tại đây)
                query = query.Where(x => x.Note != null && x.Note.Contains(filter.KieuCan));
            }

            // --- 4️⃣ Lọc theo từ khóa ---
            if (!string.IsNullOrWhiteSpace(filter.TuKhoa))
            {
                string q = filter.TuKhoa.Trim();
                query = query.Where(x =>
                    (x.Code != null && x.Code.Contains(q)) ||
                    (x.Vehicle != null && x.Vehicle.Contains(q)) ||
                    (x.DriverName != null && x.DriverName.Contains(q)) ||
                    (x.CustomerName != null && x.CustomerName.Contains(q)) ||
                    (x.ProductName != null && x.ProductName.Contains(q))
                );
            }

            // --- 5️⃣ Trả kết quả ---
            var result = await query
                .OrderByDescending(x => x.Id)
                .Take(take)
                .ToListAsync(ct);

            return result;
        }
        public async Task<ScaleStatisticDTO> GetStatisticAsync(DateTime? ngay = null, CancellationToken ct = default)
        {
            using var _db = _factory.CreateDbContext();

            var date = (ngay ?? DateTime.Today).Date;
            var nextDay = date.AddDays(1);

            // Các phiếu trong ngày
            var query = _db.TblScales
                .AsNoTracking()
                .Where(x => x.CreateDay >= date && x.CreateDay < nextDay);

            // Cân lần 1: đã cân vào nhưng chưa ra
            var lan1 = await query.CountAsync(x => x.WeightIn > 0 && (x.WeightOut == null || x.WeightOut == 0), ct);

            // Cân lần 2: đã có cả WeightIn và WeightOut
            var lan2 = await query.CountAsync(x => x.WeightOut != null && x.WeightOut > 0, ct);

            // Xe chờ cân lần 2 (có thể mở rộng logic riêng)
            var choCanLan2 = await _db.TblScales
                .CountAsync(x => x.WeightIn > 0 && (x.WeightOut == null || x.WeightOut == 0), ct);

            return new ScaleStatisticDTO
            {
                SoLan1TrongNgay = lan1,
                SoLan2TrongNgay = lan2,
                SoXeChoCanLan2 = choCanLan2
            };
        }

        public async Task<TblScale> AddAsync(TblScale v, CancellationToken ct = default)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var _db = _factory.CreateDbContext();
                v.Status = (int)ScaleStatus.Weighing;
                v.SyncFailCount = 0;
                if (v.IndexInDay == null)
                {
                    var today = DateTime.Today;
                    var lastIndex = await _db.TblScales.AsNoTracking()
                        .Where(x => x.CreateDay >= today)
                        .MaxAsync(x => (int?)x.IndexInDay) ?? 0;

                    v.IndexInDay = lastIndex + 1;
                }
                _db.TblScales.Add(v);
                await _db.SaveChangesAsync();

            });
            return v;
        }
        public async Task<TblScale> CanLan2(TblScale v, CancellationToken ct = default)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var _db = _factory.CreateDbContext();
                var scaleUpdate = await _db.TblScales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == v.Id);
                scaleUpdate.TimeOut = DateTime.Now;
                scaleUpdate.WeightOut = v.WeightOut;
                scaleUpdate.Note = v.Note;
                scaleUpdate.ProductNumber = Math.Abs((decimal)v.WeightOut - (decimal)v.WeightIn);
                scaleUpdate.Img21 = v.Img21;
                scaleUpdate.Img22 = v.Img22;
                scaleUpdate.Img23 = v.Img23;
                scaleUpdate.Status = (int)ScaleStatus.Completed;
                scaleUpdate.SyncStatus = (int)SyncStatus.NotSynced;
                _db.TblScales.Update(scaleUpdate);
                await _db.SaveChangesAsync(ct);

            });
            return v;
        }
        public async Task<TblScale> LuuPhieu(TblScale v, CancellationToken ct = default)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var _db = _factory.CreateDbContext();
                var scaleUpdate = await _db.TblScales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == v.Id);
                scaleUpdate.Vehicle = v.Vehicle;
                scaleUpdate.CustomerName = v.CustomerName;
                scaleUpdate.ProductName = v.ProductName;
                scaleUpdate.TypeId = v.TypeId;
                scaleUpdate.DriverName = v.DriverName;
                scaleUpdate.Note = v.Note;
                scaleUpdate.UpdateDay = DateTime.Now;
                _db.TblScales.Update(scaleUpdate);
                await _db.SaveChangesAsync(ct);

            });
            return v;
        }
        public async Task<TblScale> UpdateScaleAsynnc(TblScale v, CancellationToken ct = default)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var _db = _factory.CreateDbContext();
                var scaleUpdate = await _db.TblScales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == v.Id);
                scaleUpdate.Vehicle = v.Vehicle;
                scaleUpdate.CustomerName = v.CustomerName;
                scaleUpdate.ProductName = v.ProductName;
                scaleUpdate.ProductNumber = v.ProductNumber;
                scaleUpdate.ProductPrice = v.ProductPrice;
                scaleUpdate.TotalMoney = v.TotalMoney;
                scaleUpdate.Note = v.Note;
                scaleUpdate.UpdateDay = DateTime.Now;
                _db.TblScales.Update(scaleUpdate);
                await _db.SaveChangesAsync(ct);

            });
            return v;
        }
        public async Task<List<TblScale>> GetUnsyncedAsync(int take = 20, CancellationToken ct = default)
        {
            using var _db = _factory.CreateDbContext();
            var query = _db.TblScales.AsNoTracking().AsQueryable().Where(x => x.Status == 1  &&  x.SyncStatus == 0);
              
            var res = await query
                .OrderByDescending(x => x.Id)
                .Take(take)
                .ToListAsync(ct);
            var sql = query.ToQueryString();
            return res;
        }
        public async Task<TblScale> UpdateScaleAll(TblScale v, CancellationToken ct = default)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var _db = _factory.CreateDbContext();

                var existing = await _db.TblScales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == v.Id, ct);
                if (existing == null)
                    throw new Exception("Không tìm thấy bản ghi cần cập nhật");


                // Gắn bản ghi lại vào context để có thể cập nhật
                _db.Attach(v);
                v.UpdateDay = DateTime.Now;

                // Đánh dấu entity là Modified
                _db.Entry(v).State = EntityState.Modified;

                //// Cập nhật tất cả field (trừ Id)
                //_db.Entry(existing).CurrentValues.SetValues(v);

                //existing.UpdateDay = DateTime.Now; // ghi đè nếu cần

                await _db.SaveChangesAsync(ct);
            });

            return v;
        }
        //CreateScaleAsync
        public async Task<TblScale> CreateScaleAsync(TblScale v, CancellationToken ct = default)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var _db = _factory.CreateDbContext();
                if (v.IndexInDay == null)
                {
                    var today = DateTime.Today;
                    var lastIndex = await _db.TblScales.AsNoTracking()
                        .Where(x => x.CreateDay >= today)
                        .MaxAsync(x => (int?)x.IndexInDay) ?? 0;

                    v.IndexInDay = lastIndex + 1;
                }
                _db.TblScales.Add(v);
                await _db.SaveChangesAsync();

            });
            return v;
        }
        public async Task DeleteAsync(int id)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                var v = await db.TblScales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (v != null)
                {
                    db.TblScales.Remove(v);
                    await db.SaveChangesAsync();
                }
            });
        }
    }

}
