using Microsoft.UI.Windowing;
using SGS2025.Core.Services.ShareServices;
using Windows.Graphics;

namespace SGS2025Client;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;

        if (await _authService.LoginAsync(username, password))
        {
#if WINDOWS
            // Lấy handle cửa sổ
            var window = Application.Current.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(id);

            // Maximize cửa sổ
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Maximize();
            }
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