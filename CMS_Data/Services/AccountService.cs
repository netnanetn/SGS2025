using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class AccountService
    {
        private readonly IDbContextFactory<MoDaContext> _factory;
        public AccountService(IDbContextFactory<MoDaContext> factory)
        {
            _factory = factory;
        }

        public async Task<TblAcccount?> GetByIdAsync()
        {
            try
            {
                using var _db = _factory.CreateDbContext();
                var res = await _db.TblAcccounts.AsNoTracking().FirstOrDefaultAsync();
                return res;
            }
            catch (Exception e)
            {
                return new TblAcccount();
            }
        }

        public async Task<TblAcccount?> CheckLoginAsync(string username, string password)
        {
            try
            {
                using var _db = _factory.CreateDbContext();
                string hashed = UtilsService.HashPassword(password);
                var res = await _db.TblAcccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserName == username && x.PassWord == hashed && (x.Status ?? true));

                return res;
            }
            catch (Exception e)
            {
                // có thể log lỗi e.Message ở đây
                return null;
            }
        }
        public async Task<TblAcccount?> CheckLoginByHashAsync(string username, string hashed)
        {
            try
            {
                using var _db = _factory.CreateDbContext();
                var res = await _db.TblAcccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserName == username && x.PassWord == hashed && (x.Status ?? true));

                return res;
            }
            catch (Exception e)
            {
                // có thể log lỗi e.Message ở đây
                return null;
            }
        }
    }

}
