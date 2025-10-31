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
using SGS2025Client.Shared;
//using SGS2025Client.Services;
using Microsoft.Web.WebView2.Core;
using SGS2025Client.SDKCameraServices.Tvt;
using SGS2025Client.SDKCameraServices.CameraFactory;
using Microsoft.Data.Sqlite;
using SGS2025.Core.Services.ShareServices;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using CommunityToolkit.Maui;
using WinRT.Interop;
using Windows.Services.Maps;
using CMS_Data.Networks;
using SGS2025Client.Services;
using CMS_Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

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
                }) ;

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
            builder.Services.AddSingleton<IConfigService>(sp => new ConfigService(targetPath)); 

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

                // Tăng thời gian chờ khi database đang bị khóa
                // (BusyTimeout được set qua PRAGMA, DefaultTimeout chỉ áp dụng cho ADO.NET command)
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    // Cho phép nhiều process đọc/ghi cùng lúc
                    cmd.CommandText = "PRAGMA journal_mode=WAL;";
                    cmd.ExecuteNonQuery();

                    // Đợi tối đa 5 giây nếu database đang bị lock
                    cmd.CommandText = "PRAGMA busy_timeout = 5000;";
                    cmd.ExecuteNonQuery();
                }

                options.UseSqlite(connection);
            });

            builder.Services.AddSingleton<AuthService>();

            builder.Services.AddScoped<AccountService>();
            builder.Services.AddScoped<VehicleService>();
            


            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<LoadDataService>();
            builder.Services.AddScoped<ScaleService>();
            builder.Services.AddScoped<CompanyService>();

            builder.Services.AddSingleton<RazorRenderer>();
            builder.Services.AddSingleton<PdfService>();
            builder.Services.AddScoped<ImageStorageService>();
            builder.Services.AddScoped<ExcelService>();
            builder.Services.AddScoped<LicenseService>();


            // --- Đăng ký các service cần thiết ---
        
            builder.Services.AddHttpClient<IAuthOnlineService, AuthOnlineService>((sp, c) =>
            {
                var ApiBaseUrl = "http://103.109.43.40:8018/api/";
                c.BaseAddress = new Uri(ApiBaseUrl);
            });

            builder.Services.AddHttpClient<IApiService, ApiService>((sp, c) =>
            {
                var ApiBaseUrl = "http://103.109.43.40:8018/api/";
                c.BaseAddress = new Uri(ApiBaseUrl);
            });

            builder.Services.AddSingleton<INetworkStatusProvider, MauiNetworkStatusProvider>();
            builder.Services.AddSingleton<IM31Service, M31Service>();
            builder.Services.AddSingleton<IC3Service, C3Service>();
            builder.Services.AddSingleton<C3BackgroundWorker>();
            builder.Services.AddSingleton<BackgroundSyncService>();



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
            builder.Logging.AddConsole();
#endif
            builder.Services.AddBlazorWebViewDeveloperTools();
#if WINDOWS
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CameraHostView, CameraHostViewHandler>();
            });
#endif
            builder.ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
                events.AddWindows(w =>
                    w.OnWindowCreated(window =>
                    {
                        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                        var appWindow = AppWindow.GetFromWindowId(id);

                        // Đặt kích thước nhỏ cho Login
                        appWindow.Resize(new SizeInt32(800, 500));

                        // Căn giữa màn hình
                        var displayArea = DisplayArea.GetFromWindowId(id, DisplayAreaFallback.Primary);
                        var centerX = (displayArea.WorkArea.Width - 600) / 2;
                        var centerY = (displayArea.WorkArea.Height - 600) / 2;
                        appWindow.Move(new PointInt32(centerX, centerY));
                    }));
#endif
            });
            builder.UseMauiApp<App>().UseMauiCommunityToolkit();
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
