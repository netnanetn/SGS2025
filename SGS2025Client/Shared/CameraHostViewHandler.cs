#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using System.Runtime.InteropServices;

namespace SGS2025Client.Shared;

public class CameraHostViewHandler : ViewHandler<CameraHostView, FrameworkElement>
{
    public static IPropertyMapper<CameraHostView, CameraHostViewHandler> Mapper =
        new PropertyMapper<CameraHostView, CameraHostViewHandler>(ViewHandler.ViewMapper);

    public CameraHostViewHandler() : base(Mapper)
    {
    }
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
       int dwExStyle,
       string lpClassName,
       string lpWindowName,
       int dwStyle,
       int x,
       int y,
       int nWidth,
       int nHeight,
       IntPtr hWndParent,
       IntPtr hMenu,
       IntPtr hInstance,
       IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;

    private IntPtr _childHwnd;
    private const int WS_BORDER = 0x00800000;
    private const int WS_OVERLAPPED = 0x00000000;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_SYSMENU = 0x00080000;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_MAXIMIZEBOX = 0x00010000;

    private const int WS_OVERLAPPEDWINDOW =
        WS_OVERLAPPED |
        WS_CAPTION |
        WS_SYSMENU |
        WS_THICKFRAME |
        WS_MINIMIZEBOX |
        WS_MAXIMIZEBOX;

    protected override FrameworkElement CreatePlatformView()
    {
        var panel = new Microsoft.UI.Xaml.Controls.Grid();

        panel.Loaded += (s, e) =>
        { 

            // Lấy ra MAUI Window hiện tại
            var mauiWindow = Microsoft.Maui.Controls.Application.Current.Windows[0];

            // Lấy WinUI Window từ Handler
            //var winuiWindow = (Microsoft.UI.Xaml.Window)mauiWindow.Handler.PlatformView;
            var winuiWindow = Microsoft.Maui.Controls.Application.Current.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window;

            // Lấy HWND của WinUI Window
            var hwndParent = WinRT.Interop.WindowNative.GetWindowHandle(winuiWindow);
            //_childHwnd = CreateWindowEx(
            //    0,
            //    "STATIC",
            //    "Hello HWND!",
            //    WS_VISIBLE | WS_OVERLAPPEDWINDOW,
            //    100, 100, 400, 300,
            //    IntPtr.Zero, // không truyền parent
            //    IntPtr.Zero,
            //    IntPtr.Zero,
            //    IntPtr.Zero
            //);
            // ✅ Tạo HWND con
            _childHwnd = CreateWindowEx(
                0,
                "STATIC",    // thay vì STATIC
                "Hello HWND!",
                WS_CHILD | WS_VISIBLE,
                0, 0,
                (int)panel.ActualWidth,
                (int)panel.ActualHeight,
                hwndParent,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);
            if (_childHwnd == IntPtr.Zero)
            {
                var err = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"❌ CreateWindowEx failed, error={err}");
            }
            SetWindowText(_childHwnd, "Hello from HWND!");
            VirtualView.Hwnd = _childHwnd;
            // Cập nhật kích thước
            panel.SizeChanged += (s, e) =>
            {
                if (_childHwnd != IntPtr.Zero)
                {
                    SetWindowPos(
                        _childHwnd,
                        new IntPtr(-1), // HWND_TOP
                        0,
                        0,
                        (int)e.NewSize.Width,
                        (int)e.NewSize.Height,
                        0);
                }
            };
        };

        return panel;
    }
}
#endif