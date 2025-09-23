using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class CompanyService
    {
        private readonly IDbContextFactory<MoDaContext> _factory;
        public CompanyService(IDbContextFactory<MoDaContext> factory)
        {
            _factory = factory;
        }

        public async Task<TblCompany?> GetByIdAsync()
        {
            try
            {
                using var _db = _factory.CreateDbContext();
                var res = await _db.TblCompanies.AsNoTracking().FirstOrDefaultAsync();
                return res;
            }
            catch (Exception e)
            {
                return new TblCompany();
            }
        }

        public async Task<TblCompany?> UpdateAsync(TblCompany company, string user)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var _db = _factory.CreateDbContext();
                var existing = await _db.TblCompanies.FirstOrDefaultAsync(x => x.Id == company.Id);
                if (existing == null) return null;

                existing.Code = company.Code;
                existing.Name = company.Name;
                existing.Address = company.Address;
                existing.Phone = company.Phone;
                existing.Email = company.Email;
                existing.Note = company.Note;
                existing.AccOnline = company.AccOnline;
                existing.PassOnline = company.PassOnline;

                existing.UpdateDay = DateTime.Now;
                existing.UpdateBy = user;

                await _db.SaveChangesAsync();
                return existing;
            });
            return company;
        }

    }

}
