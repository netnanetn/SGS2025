using CMS_Data.Models;
using CMS_Data.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using SGS2025Client.Components.HelperService;
using SGS2025Client.Components.LibPDFs;
using SGS2025Client.SDKCameraServices.Dahua;
using SGS2025Client.SDKCameraServices.Hik;
using SGS2025Client.Services;
using SGS2025Client.Shared;
//using SGS2025Client.Services;
using Microsoft.Web.WebView2.Core;
using SGS2025Client.SDKCameraServices.Tvt;
using SGS2025Client.SDKCameraServices.CameraFactory;
using Microsoft.Data.Sqlite;

namespace SGS2025Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // 🔹 cấu hình hệ thống
            var folder = @"C:\TVS\Config";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = "appsettings.sgs.json";
            var targetPath = Path.Combine(folder, fileName);

            // Nếu chưa có file config thì copy từ AppPackage (Resources)
            if (!File.Exists(targetPath))
            {
                using var stream = FileSystem.OpenAppPackageFileAsync(fileName).Result;
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();
                File.WriteAllText(targetPath, content);
            }
            builder.Services.AddSingleton(new ConfigService(targetPath));


            // 🔹 Đường dẫn SQLite
            var dbPath = Path.Combine("E:\\MyProject\\SGS2025\\Database", "SGS2025OFFLINE.db");

            // 🔹 Đăng ký DbContext vào DI
            //builder.Services.AddDbContext<MoDaContext>(options =>
            //    options.UseSqlite($"Data Source={dbPath}"));
            // 🔹 Đăng ký DbContextFactory thay vì DbContext
             
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

            builder.Services.AddSingleton<RazorRenderer>();
            builder.Services.AddSingleton<PdfService>();
            builder.Services.AddScoped<ImageStorageService>();


            // ✅ Đăng ký DI (Dependency Injection)
            //builder.Services.AddSingleton<CameraService>();
            builder.Services.AddSingleton<FactoryCameraService>();
            builder.Services.AddSingleton<HikvisionCameraService>();
            builder.Services.AddSingleton<DahuaCameraService>();
            builder.Services.AddSingleton<TvtCameraService>();


             builder.Services.AddSingleton<WeighingScaleService>(); 

            // Add services to the container.
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddBlazorWebViewDeveloperTools();
#if WINDOWS
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CameraHostView, CameraHostViewHandler>();
            });
#endif
          
            var app =  builder.Build();

            // 🔹 Pre-warm Razor template (compile sẵn, lần in đầu tiên sẽ nhanh)
            Task.Run(async () =>
            {
                try
                {
                    var renderer = app.Services.GetRequiredService<RazorRenderer>();
                    await renderer.RenderTemplateAsync("ScalePdfTemplate6Img.cshtml", new TblScale());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Pre-warm Razor template failed: {ex.Message}");
                }
            });

            return app;

        }
    }
}
