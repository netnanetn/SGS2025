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
        public async Task<TblScale> AddAsync(TblScale v, CancellationToken ct = default)
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
                await _db.SaveChangesAsync(ct);

            });
            return v;
        }
    }

}
