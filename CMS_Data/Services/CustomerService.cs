using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class CustomerService
    {
        private readonly IDbContextFactory<MoDaContext> _factory;
        public CustomerService(IDbContextFactory<MoDaContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<TblCustomer>> SearchAsync(string? q, int take = 20, CancellationToken ct = default)
        {
            using var _db = _factory.CreateDbContext();
            var query = _db.TblCustomers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.Name.Contains(q) ||
                    (x.Code != null && x.Code.Contains(q)) ||
                    (x.Address != null && x.Address.Contains(q)));
            }
            return await query
                .OrderBy(x => x.Name)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<TblCustomer> AddAsync(TblCustomer v, CancellationToken ct = default)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                db.TblCustomers.Add(v);
                await db.SaveChangesAsync(ct);
            });
            return v;
        }
    }

}
