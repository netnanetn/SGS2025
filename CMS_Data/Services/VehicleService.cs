using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class VehicleService
    {
        private readonly MoDaContext _db;
        public VehicleService(MoDaContext db) => _db = db;

        public async Task<List<TblVehicle>> SearchAsync(string? q, int take = 20, CancellationToken ct = default)
        {
            var query = _db.TblVehicles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.VehiceCode.Contains(q) ||
                    (x.RfidCode != null && x.RfidCode.Contains(q)) ||
                    (x.DriverName != null && x.DriverName.Contains(q)));
            }
            return await query
                .OrderBy(x => x.VehiceCode)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<TblVehicle> AddAsync(TblVehicle v, CancellationToken ct = default)
        {
            _db.TblVehicles.Add(v);
            await _db.SaveChangesAsync(ct);
            return v;
        }
    }

}
