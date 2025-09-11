using Microsoft.UI.Xaml;
using SGS2025Client.SDKCameraServices.Dahua;
using WinRT.Interop;

namespace SGS2025Client.Controls.Dahua;

public partial class CameraDahuaHostView : ContentPage
{
    private DahuaCameraService2 _cameraService = new();

    public CameraDahuaHostView()
    {
        InitializeComponent(); 
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
     
        Cam1.Loaded += (s, e) =>
        {
#if WINDOWS
            if (Cam1.Hwnd != IntPtr.Zero)
            {
                _cameraService.AddCamera("cam1", "192.168.1.109", 37777, "admin", "abcd@1234", Cam1.Hwnd, 0);
            }
#endif
        };
#if WINDOWS
        //if (Cam1.Hwnd != IntPtr.Zero)
        //    _cameraService.AddCamera("cam1", "192.168.1.109", 37777, "admin", "abcd@1234", Cam1.Hwnd, 0);

        //if (Cam2.Hwnd != IntPtr.Zero)
        //    _cameraService.AddCamera("cam2", "192.168.1.109", 37777, "admin", "abcd@1234", Cam2.Hwnd, 0);

        //if (Cam3.Hwnd != IntPtr.Zero)
        //    _cameraService.AddCamera("cam3", "192.168.1.109", 37777, "admin", "abcd@1234", Cam3.Hwnd, 0);

        //if (Cam4.Hwnd != IntPtr.Zero)
        //    _cameraService.AddCamera("cam4", "192.168.1.109", 37777, "admin", "abcd@1234", Cam4.Hwnd, 0);
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cameraService.StopAll();
    }
}