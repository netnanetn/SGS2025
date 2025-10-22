using CMS_Data.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SGS2025.Core.Services.ShareServices;
using SGS2025Client.Components.Pages;
using SGS2025Client.SDKCameraServices.CameraFactory;
using SGS2025Client.Services;

namespace SGS2025Client
{
    public partial class App : Application
    {
        private readonly BackgroundSyncService _syncService;
        private readonly AuthService _authService;
        private readonly LicenseService _licenseService;
        public App(AuthService authService, LicenseService licenseService, BackgroundSyncService syncService)
        {
            InitializeComponent();
           _licenseService = licenseService;
            _authService = authService;
            _syncService = syncService;

             

            bool valid = _licenseService.ValidateLicense();

            if (!valid)
            {
                string hwId = _licenseService.GetLocalHardwareId();
                MainPage = new NoLicensePage(hwId, _licenseService);
            }
            else
            {
                // Bắt đầu auto sync khi mở app
                _syncService.StartAutoSync(60);
                MainPage = new NavigationPage(new LoginPage(_authService));
            }
             

        }

         
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Destroying += (s, e) =>
            {
                var service = Current.Handler.MauiContext.Services.GetService<FactoryCameraService>();
                service?.StopAll();
                // Dọn WebView2
                foreach (var page in window.Page.Navigation.NavigationStack)
                {
                    if (page is ContentPage cp)
                    {
                        foreach (var view in cp.Content.LogicalChildren)
                        {
                            if (view is BlazorWebView bwv)
                                bwv.Handler?.DisconnectHandler(); // force dispose
                        }
                    }
                }
                foreach (var process in System.Diagnostics.Process.GetProcessesByName("chrome"))
                {
                    if (process.MainModule.FileName.Contains("AppX\\Chrome")) // chỉ kill chrome đi kèm app
                        process.Kill();
                }

            };

            return window;
        }
    }
}
