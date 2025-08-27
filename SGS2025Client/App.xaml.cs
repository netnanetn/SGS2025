namespace SGS2025Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // MainPage = new MainPage();
            MainPage = new NavigationPage(new MainPage()); // MainPage có BlazorWebView
        }
    }
}
