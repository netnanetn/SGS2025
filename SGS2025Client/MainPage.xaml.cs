using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace SGS2025Client
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
#if WINDOWS
            blazorWebView.BlazorWebViewInitialized += (s, e) =>
            {
                var coreWebView2 = e.WebView.CoreWebView2;
                if (coreWebView2 != null)
                {
                    // Map host ảo local.tvs -> thư mục ảnh
                    coreWebView2.SetVirtualHostNameToFolderMapping(
                        "local.tvs",
                        @"C:\TVS\Images",   // thư mục chứa ảnh
                        CoreWebView2HostResourceAccessKind.Allow
                    );
                }
            };
#endif
        }
    }
}
