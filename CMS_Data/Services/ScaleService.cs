using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class ScaleService
    {
        private readonly MoDaContext _db;
        public ScaleService(MoDaContext db) => _db = db;

        public async Task<List<TblScale>> SearchAsync(string? q, int take = 20, CancellationToken ct = default)
        {
            var query = _db.TblScales.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.Code.Contains(q) ||
                    (x.CustomerName != null && x.CustomerName.Contains(q)) ||
                    (x.ProductName != null && x.ProductName.Contains(q)));
            }
            return await query
                .OrderByDescending(x => x.Id)
                .Take(take)
                .ToListAsync(ct);
        }
        public async Task<TblScale> AddAsync(TblScale v, CancellationToken ct = default)
        {
            // Nếu cần số phiếu theo ngày
            if (v.IndexInDay == null)
            {
                var today = DateTime.Today;
                var lastIndex = await _db.TblScales
                    .Where(x => x.CreateDay >= today)
                    .MaxAsync(x => (int?)x.IndexInDay) ?? 0;

                v.IndexInDay = lastIndex + 1;
            }

            _db.TblScales.Add(v);
            await _db.SaveChangesAsync(ct);
            return v;
        }
        public async Task<TblScale> CanLan2(TblScale v, CancellationToken ct = default)
        {
            var scaleUpdate = await _db.TblScales.FirstOrDefaultAsync(x => x.Id == v.Id);
            scaleUpdate.TimeOut = DateTime.Now;
            scaleUpdate.WeightOut = v.WeightOut;
            scaleUpdate.Note = v.Note;
            scaleUpdate.ProductNumber = Math.Abs((decimal)v.WeightOut - (decimal)v.WeightIn);
            await _db.SaveChangesAsync(ct);
            return v;
        }
        public async Task<TblScale> LuuPhieu(TblScale v, CancellationToken ct = default)
        {
            var scaleUpdate = await _db.TblScales.FirstOrDefaultAsync(x => x.Id == v.Id);
            scaleUpdate.Vehicle = v.Vehicle;
            scaleUpdate.CustomerName = v.CustomerName;
            scaleUpdate.ProductName = v.ProductName;
            scaleUpdate.TypeId = v.TypeId;
            scaleUpdate.DriverName = v.DriverName;
            scaleUpdate.Note = v.Note;
            scaleUpdate.UpdateDay = DateTime.Now;
            await _db.SaveChangesAsync(ct);
            return v;
        }
    }

}
