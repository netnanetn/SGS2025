namespace SGS2025Client;

public partial class NoLicensePage : ContentPage
{
    private readonly string _hardwareId;
    private readonly LicenseService _licenseService;

    public NoLicensePage(string hardwareId, LicenseService licenseService)
    {
        InitializeComponent();
        _hardwareId = hardwareId;
        HardwareIdEntry.Text = hardwareId;
        _licenseService = licenseService;
    }

    private async void OnCopyClicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(_hardwareId);
        await DisplayAlert("Đã copy", "Hardware ID đã được copy vào clipboard.", "OK");
    }
    private async void OnImportLicenseClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Chọn license file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".lic" } },
            { DevicePlatform.Android, new[] { "application/octet-stream" } },
            { DevicePlatform.iOS, new[] { "public.data" } },
            { DevicePlatform.MacCatalyst, new[] { "public.data" } }
        })
        });

        if (result != null)
        {
            using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var licenseContent = await reader.ReadToEndAsync();

            // Lưu file vào local storage
            var licensePath = Path.Combine(FileSystem.AppDataDirectory, "license.lic");
            File.WriteAllText(licensePath, licenseContent);

            await DisplayAlert("Thành công", "Đã import license. Khởi động lại ứng dụng", "OK");

            // Thử validate lại
            var valid =   _licenseService.ValidateLicense();
            if (valid)
            {
                await DisplayAlert("Thành công", "Đã kiểm tra license hợp lệ. Vui lòng Khởi động lại ứng dụng", "OK");
            }
            else
            {
                await DisplayAlert("Lỗi", "License không hợp lệ.", "OK");
            }
        }
    }
}