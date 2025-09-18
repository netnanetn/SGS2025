using CMS_Data.Models;
using CMS_Data.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SGS2025.Core.Services.ShareServices;
using SGS2025BlazorServer.Components;
using SGS2025BlazorServer.Components.LibPDFs;

namespace SGS2025BlazorServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 cấu hình hệ thống
            var folder = @"C:\TVS\Config";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = "appsettings.sgs.json";
            var targetPath = Path.Combine(folder, fileName);

            
            builder.Services.AddSingleton(new ConfigService(targetPath));

            // 🔹 Đường dẫn SQLite
            //var dbPath = Path.Combine("E:\\MyProject\\SGS2025\\Database", "SGS2025OFFLINE.db");
            var basePath = AppContext.BaseDirectory;
            var dbPath = Path.Combine(basePath, "Database", "SGS2025OFFLINE.db");

            // 🔹 Đăng ký DbContext vào DI
            builder.Services.AddDbContextFactory<MoDaContext>(options =>
            {
                var connectionString = $"Data Source={dbPath}";
                var connection = new SqliteConnection(connectionString);

                // Set BusyTimeout = 5 giây
                connection.DefaultTimeout = 5; // giây

                options.UseSqlite(connection);
            });


            builder.Services.AddScoped<VehicleService>();

            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<LoadDataService>();
            builder.Services.AddScoped<ScaleService>(); 
            builder.Services.AddSingleton<PdfService>();
            builder.Services.AddScoped<ImageStorageService>();
            builder.Services.AddScoped<ExcelVehicleService>();
             


            builder.Services.AddSingleton<WeighingScaleService>();

            // Add services to the container.
            builder.Services.AddBlazorBootstrap();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
           
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
