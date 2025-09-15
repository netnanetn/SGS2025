using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class ProductService
    {
        private readonly IDbContextFactory<MoDaContext> _factory;
        public ProductService(IDbContextFactory<MoDaContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<TblProduct>> SearchAsync(string? q, int take = 20, CancellationToken ct = default)
        {
            using var _db = _factory.CreateDbContext();
            var query = _db.TblProducts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.Name.Contains(q) ||
                    (x.Code != null && x.Code.Contains(q)) );
            }
            return await query
                .OrderBy(x => x.Name)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<TblProduct> AddAsync(TblProduct v, CancellationToken ct = default)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                db.TblProducts.Add(v);
                await db.SaveChangesAsync();

            });
            return v;
        }
    }

}
