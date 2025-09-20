using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SGS2025.Core.Services.ShareServices;
using SGS2025Client.Components.Pages;
using SGS2025Client.SDKCameraServices.CameraFactory;

namespace SGS2025Client
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        public App(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            MainPage = new NavigationPage(new LoginPage(_authService));
            // MainPage = new MainPage();
            // MainPage = new NavigationPage(new MainPage()); // MainPage có BlazorWebView
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
