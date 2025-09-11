using CMS_Data.Models;
using CMS_Data.Services;
using Microsoft.EntityFrameworkCore;
using SGS2025BlazorServer.Components;
using SGS2025BlazorServer.Components.LibPDFs;

namespace SGS2025BlazorServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // 🔹 Đường dẫn SQLite
            //var dbPath = Path.Combine("E:\\MyProject\\SGS2025\\Database", "SGS2025OFFLINE.db");
            var basePath = AppContext.BaseDirectory;
            var dbPath = Path.Combine(basePath, "Database", "SGS2025OFFLINE.db");

            // 🔹 Đăng ký DbContext vào DI
            builder.Services.AddDbContext<MoDaContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));
            builder.Services.AddScoped<VehicleService>();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<LoadDataService>();
            builder.Services.AddScoped<ScaleService>();

            builder.Services.AddScoped<PdfService>();

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
