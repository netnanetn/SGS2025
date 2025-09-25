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
        //
        // 
        public async Task<List<TblCustomer>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.TblCustomers
                .AsNoTracking()
                .OrderBy(v => v.Id)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<TblCustomer?> GetByIdAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            return await db.TblCustomers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<TblCustomer> AddAsync(TblCustomer v)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                db.TblCustomers.Add(v);
                await db.SaveChangesAsync();

            });
            return v;
        }

        public async Task UpdateAsync(TblCustomer v)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                db.TblCustomers.Update(v);
                await db.SaveChangesAsync();
            });
        }

        public async Task DeleteAsync(int id)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                var v = await db.TblCustomers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (v != null)
                {
                    db.TblCustomers.Remove(v);
                    await db.SaveChangesAsync();
                }
            });
        }
        public async Task Seeding()
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                var demoCustomers = new List<TblCustomer>
                {
                    new TblCustomer { Code = "C001", Name = "Nguyễn Văn A", Address = "Hà Nội", Phone = "0912345678", Email = "vana@example.com", DebtFirst = 0, BankAccount = "123456789", Note = "Khách thân thiết", CompanyId = 1, CreateDay = DateTime.Now, CreateBy = "admin" },
                    new TblCustomer { Code = "C002", Name = "Trần Thị B", Address = "TP HCM", Phone = "0987654321", Email = "thib@example.com", DebtFirst = 500000, BankAccount = "987654321", Note = "", CompanyId = 1, CreateDay = DateTime.Now, CreateBy = "admin" },
                    new TblCustomer { Code = "C003", Name = "Công ty Cổ phần Xây dựng", Address = "Đà Nẵng", Phone = "02363712345", Email = "info@congtyc.vn", DebtFirst = 2500000, BankAccount = "555666777", Note = "Khách doanh nghiệp", CompanyId = 2, CreateDay = DateTime.Now, CreateBy = "user1" },
                    new TblCustomer { Code = "C004", Name = "Phạm Văn D", Address = "Cần Thơ", Phone = "0909123456", Email = "vand@example.com", DebtFirst = 0, BankAccount = "333222111", Note = "Khách mới", CompanyId = 2, CreateDay = DateTime.Now, CreateBy = "user2" },
                    new TblCustomer { Code = "C005", Name = "Lê Thị E", Address = "Hải Phòng", Phone = "0938123456", Email = "the@example.com", DebtFirst = 1200000, BankAccount = "444555666", Note = "", CompanyId = 3, CreateDay = DateTime.Now, CreateBy = "user3" }
                };

                await db.TblCustomers.AddRangeAsync(demoCustomers);
                await db.SaveChangesAsync();
            });
        }
    }

}
