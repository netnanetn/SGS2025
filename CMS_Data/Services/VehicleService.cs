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
        private readonly IDbContextFactory<MoDaContext> _factory;
        public VehicleService(IDbContextFactory<MoDaContext> factory)
        {
            _factory = factory;
        } 

        public async Task<List<TblVehicle>> SearchAsync(string? q, int take = 20, CancellationToken ct = default)
        {
            using var _db = _factory.CreateDbContext();
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


        //
        public async Task<List<TblVehicle>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.TblVehicles
                .AsNoTracking()
                .OrderBy(v => v.Id)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<TblVehicle?> GetByIdAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            return await db.TblVehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<TblVehicle> AddAsync(TblVehicle vehicle)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                db.TblVehicles.Add(vehicle);
                await db.SaveChangesAsync();
                
            });
            return vehicle;
        }

        public async Task UpdateAsync(TblVehicle vehicle)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                db.TblVehicles.Update(vehicle);
                await db.SaveChangesAsync();
            });
        }

        public async Task DeleteAsync(int id)
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                var v = await db.TblVehicles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (v != null)
                {
                    db.TblVehicles.Remove(v);
                    await db.SaveChangesAsync();
                }
            });
        }
        public async Task Seeding()
        {
            await SQLiteWriteLock.RunAsync(async () =>
            {
                using var db = _factory.CreateDbContext();
                var demoVehicles = new List<TblVehicle>
                {
                    new TblVehicle { VehiceCode = "51A-12345", RfidCode = "RF001", DriverName = "Nguyễn Văn A", DriverPhone = "0901234567", TonnageDefault = 15.5, TransportationCompany = "Công ty Vận tải A", Status = true, CreateDay = DateTime.Now, CreateBy = "admin" },
                    new TblVehicle { VehiceCode = "51B-67890", RfidCode = "RF002", DriverName = "Trần Văn B", DriverPhone = "0912345678", TonnageDefault = 12, TransportationCompany = "Công ty Vận tải B", Status = true, CreateDay = DateTime.Now, CreateBy = "admin" },
                    new TblVehicle { VehiceCode = "60C-11223", RfidCode = "RF003", DriverName = "Lê Văn C", DriverPhone = "0987654321", TonnageDefault = 10, TransportationCompany = "Công ty Vận tải C", Status = false, CreateDay = DateTime.Now, CreateBy = "user1" },
                    new TblVehicle { VehiceCode = "29D-44556", RfidCode = "RF004", DriverName = "Phạm Văn D", DriverPhone = "0978123456", TonnageDefault = 8.5, TransportationCompany = "Công ty Vận tải D", Status = true, CreateDay = DateTime.Now, CreateBy = "user2" },
                    new TblVehicle { VehiceCode = "30E-77889", RfidCode = "RF005", DriverName = "Hoàng Văn E", DriverPhone = "0932123456", TonnageDefault = 20, TransportationCompany = "Công ty Vận tải E", Status = true, CreateDay = DateTime.Now, CreateBy = "user3" },
                    new TblVehicle { VehiceCode = "43F-99001", RfidCode = "RF006", DriverName = "Vũ Văn F", DriverPhone = "0922123456", TonnageDefault = 18, TransportationCompany = "Công ty Vận tải F", Status = true, CreateDay = DateTime.Now, CreateBy = "user4" },
                    new TblVehicle { VehiceCode = "65G-33445", RfidCode = "RF007", DriverName = "Đỗ Văn G", DriverPhone = "0911123456", TonnageDefault = 14, TransportationCompany = "Công ty Vận tải G", Status = false, CreateDay = DateTime.Now, CreateBy = "user5" },
                    new TblVehicle { VehiceCode = "79H-66778", RfidCode = "RF008", DriverName = "Bùi Văn H", DriverPhone = "0965123456", TonnageDefault = 16, TransportationCompany = "Công ty Vận tải H", Status = true, CreateDay = DateTime.Now, CreateBy = "user6" },
                    new TblVehicle { VehiceCode = "36J-88990", RfidCode = "RF009", DriverName = "Ngô Văn J", DriverPhone = "0944123456", TonnageDefault = 11.2, TransportationCompany = "Công ty Vận tải J", Status = true, CreateDay = DateTime.Now, CreateBy = "user7" },
                    new TblVehicle { VehiceCode = "47K-55667", RfidCode = "RF010", DriverName = "Mai Văn K", DriverPhone = "0955123456", TonnageDefault = 9.5, TransportationCompany = "Công ty Vận tải K", Status = true, CreateDay = DateTime.Now, CreateBy = "user8" }
                };
                await db.TblVehicles.AddRangeAsync(demoVehicles);
                await db.SaveChangesAsync();
            });
        }
    }

}
