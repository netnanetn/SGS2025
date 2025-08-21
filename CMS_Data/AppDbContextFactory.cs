using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMS_Data.Models;

namespace CMS_Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<MoDaContext>
    {
        public MoDaContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MoDaContext>();

            // ✅ Chỉ định provider là SQLite
            optionsBuilder.UseSqlite("Data Source=SGS2025OFFLINE.db");

            return new MoDaContext(optionsBuilder.Options);
        }
    }
}
