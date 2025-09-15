using CMS_Data.ModelDTO;
using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data.Services
{
    public class LoadDataService
    {
        private readonly IDbContextFactory<MoDaContext> _factory;
        public LoadDataService(IDbContextFactory<MoDaContext> factory)
        {
            _factory = factory;
        }

        public async Task<FilterInitViewModel> LoadAllData( CancellationToken ct = default)
        {
            using var _db = _factory.CreateDbContext();
            var model = new FilterInitViewModel
            {
                Products = await _db.TblProducts.AsNoTracking().ToListAsync(),
                Customers = await _db.TblCustomers.AsNoTracking().ToListAsync(),
                Vehicles = await _db.TblVehicles.AsNoTracking().ToListAsync()
            };
            return model;
        }
         
    }
}
