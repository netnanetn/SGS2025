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
        private readonly MoDaContext _db;
        public LoadDataService(MoDaContext db) => _db = db;

        public async Task<FilterInitViewModel> LoadAllData( CancellationToken ct = default)
        {
            var model = new FilterInitViewModel
            {
                Products = await _db.TblProducts.ToListAsync(),
                Customers = await _db.TblCustomers.ToListAsync(),
                Vehicles = await _db.TblVehicles.ToListAsync()
            };
            return model;
        }
         
    }
}
