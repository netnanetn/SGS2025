using CMS_Data.Services;
using Microsoft.UI.Windowing;
using SGS2025.Core.Services.ShareServices;
using System.Collections.Generic;
using Windows.Graphics;

namespace SGS2025Client;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private bool _isFirstAppearing = true;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        // Bắt đầu check auto login
       // _ = TryAutoLogin();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isFirstAppearing)
        {
            _isFirstAppearing = false;
            await TryAutoLogin();
        }
    }

    private async Task TryAutoLogin()
    {
        try
        {
            var username = await SecureStorage.GetAsync("username");
            var passwordHash = await SecureStorage.GetAsync("password_hash");

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(passwordHash))
            {
                var account = await _authService.LoginByHashAsync(username, passwordHash);
                if (account != null)
                {
#if WINDOWS
                    MaximizeWindow();
#endif
                    // Nếu login thành công → sang MainPage
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
 
                        // Chuyển sang MainPage
                        Application.Current.MainPage = new NavigationPage(new MainPage());
                    });
                    return;
                }
            }

            // Nếu fail thì để nguyên login form
        }
        catch
        {
            // Trường hợp SecureStorage lỗi → để nguyên login form
        }
    }
#if WINDOWS
    private async void MaximizeWindow()
    {
        try
        {
            await Task.Delay(300); // đợi cửa sổ khởi tạo xong
            var window = Application.Current.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);

            if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
            {
                presenter.Maximize();
            }
        }
        catch (Exception e)
        {
             
        }
    }
#endif
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;

        if (await _authService.LoginAsync(username, password))
        {
            string hashed = UtilsService.HashPassword(password);
            // Lưu username + pass đã hash
            await SecureStorage.SetAsync("username", username);
            await SecureStorage.SetAsync("password_hash", hashed);
#if WINDOWS
            MaximizeWindow();
#endif
            // Gọi hàm trong App để chuyển sang AppShell
            if (Application.Current is App app)
            {
                app.MainPage = new NavigationPage(new MainPage());
            }
        }
        else
        {
            await DisplayAlert("Thông báo", "Sai tài khoản hoặc mật khẩu", "OK");
        }
    }
}