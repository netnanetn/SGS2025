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

        // 
        public async Task<List<TblProduct>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.TblProducts
                .AsNoTracking()
                .OrderBy(v => v.Id)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<TblProduct?> GetByIdAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            return await db.TblProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<TblProduct> AddAsync(TblProduct v)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                db.TblProducts.Add(v);
                await db.SaveChangesAsync();

            });
            return v;
        }

        public async Task UpdateAsync(TblProduct vehicle)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                db.TblProducts.Update(vehicle);
                await db.SaveChangesAsync();
            });
        }

        public async Task DeleteAsync(int id)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                var v = await db.TblProducts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (v != null)
                {
                    db.TblProducts.Remove(v);
                    await db.SaveChangesAsync();
                }
            });
        }
        public async Task Seeding()
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                var demoProducts = new List<TblProduct>
                {
                    new TblProduct { Code = "P001", Name = "Xi măng Holcim", UnitId = 1, UnitName = "Bao", Proportion = 1.0m, PriceInput = 75000, PriceOutput = 85000, TypeProduct = 1, CompanyId = 1, CreateDay = DateTime.Now, CreateBy = "admin" },
                    new TblProduct { Code = "P002", Name = "Cát vàng", UnitId = 2, UnitName = "Khối", Proportion = 1.0m, PriceInput = 250000, PriceOutput = 280000, TypeProduct = 2, CompanyId = 1, CreateDay = DateTime.Now, CreateBy = "admin" },
                    new TblProduct { Code = "P003", Name = "Đá 1x2", UnitId = 2, UnitName = "Khối", Proportion = 1.0m, PriceInput = 300000, PriceOutput = 330000, TypeProduct = 2, CompanyId = 2, CreateDay = DateTime.Now, CreateBy = "user1" },
                    new TblProduct { Code = "P004", Name = "Thép cây D16", UnitId = 3, UnitName = "Kg", Proportion = 1.0m, PriceInput = 18500, PriceOutput = 20000, TypeProduct = 3, CompanyId = 2, CreateDay = DateTime.Now, CreateBy = "user2" },
                    new TblProduct { Code = "P005", Name = "Gạch đỏ ống", UnitId = 4, UnitName = "Viên", Proportion = 1.0m, PriceInput = 1200, PriceOutput = 1500, TypeProduct = 4, CompanyId = 3, CreateDay = DateTime.Now, CreateBy = "user3" },
                    new TblProduct { Code = "P006", Name = "Gạch men 60x60", UnitId = 5, UnitName = "M2", Proportion = 1.0m, PriceInput = 95000, PriceOutput = 120000, TypeProduct = 5, CompanyId = 3, CreateDay = DateTime.Now, CreateBy = "user4" },
                    new TblProduct { Code = "P007", Name = "Sơn Dulux nội thất", UnitId = 6, UnitName = "Thùng", Proportion = 1.0m, PriceInput = 750000, PriceOutput = 890000, TypeProduct = 6, CompanyId = 4, CreateDay = DateTime.Now, CreateBy = "user5" },
                    new TblProduct { Code = "P008", Name = "Ống nước PVC 90", UnitId = 3, UnitName = "Mét", Proportion = 1.0m, PriceInput = 28000, PriceOutput = 35000, TypeProduct = 7, CompanyId = 4, CreateDay = DateTime.Now, CreateBy = "user6" },
                    new TblProduct { Code = "P009", Name = "Cửa thép vân gỗ", UnitId = 7, UnitName = "Cái", Proportion = 1.0m, PriceInput = 2500000, PriceOutput = 2800000, TypeProduct = 8, CompanyId = 5, CreateDay = DateTime.Now, CreateBy = "user7" },
                    new TblProduct { Code = "P010", Name = "Đèn led 18W", UnitId = 7, UnitName = "Cái", Proportion = 1.0m, PriceInput = 75000, PriceOutput = 95000, TypeProduct = 9, CompanyId = 5, CreateDay = DateTime.Now, CreateBy = "user8" }
                };
                await db.TblProducts.AddRangeAsync(demoProducts);
                await db.SaveChangesAsync();
            });
        }
    }

}
