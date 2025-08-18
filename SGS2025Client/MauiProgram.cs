using Microsoft.Extensions.Logging;
using SGS2025Client.SDKCameraServices.Dahua;
using SGS2025Client.SDKCameraServices.Hik;
using SGS2025Client.Services;
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

            // ✅ Đăng ký DI (Dependency Injection)
           //builder.Services.AddSingleton<CameraService>();
            builder.Services.AddSingleton<HikvisionCameraService>();
            builder.Services.AddSingleton<DahuaCameraService>();

            // Add services to the container.
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
