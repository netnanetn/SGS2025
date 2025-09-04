using CMS_Data.Models;
using CMS_Data.Services;
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
            builder.Services.AddDbContext<MoDaContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));
            builder.Services.AddScoped<VehicleService>();
              
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<LoadDataService>();
            builder.Services.AddScoped<ScaleService>();

            builder.Services.AddSingleton<RazorRenderer>();
            builder.Services.AddScoped<PdfService>();
              

            // ✅ Đăng ký DI (Dependency Injection)
            //builder.Services.AddSingleton<CameraService>();
            builder.Services.AddSingleton<HikvisionCameraService>();
            builder.Services.AddSingleton<DahuaCameraService>();

            builder.Services.AddSingleton<DahuaCameraService2>();

            // Add services to the container.
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

#if WINDOWS
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CameraHostView, CameraHostViewHandler>();
            });
#endif 

            return builder.Build();
        }
    }
}
