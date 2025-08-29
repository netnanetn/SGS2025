using CMS_Data.Models;
using PuppeteerSharp;
using System.Text;
using System.Threading.Tasks;
namespace SGS2025BlazorServer.Components.LibPDFs
{
    public class PdfService
    {
        public async Task<byte[]> GeneratePdfAsync(TblScale scale, string pageSize = "A4")
        {
            var htmlContent = $@"
            <!DOCTYPE html>
            <html lang='vi'>
            <head>
                <meta charset='UTF-8'>
                <title>In thông tin xe</title>
                <link href='https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap' rel='stylesheet'>
                <style>
                    @page {{ size: {pageSize}; margin: 20mm; }}
                    body {{ 
                        font-family: 'Roboto', Arial, sans-serif; 
                        font-size: 12pt; 
                        color: #333; 
                    }}
                    .container {{ 
                       
                        max-width: 100%; 
                        margin: 0 auto; 
                        padding: 20px; 
                        border: 2px solid #007bff; 
                        border-radius: 10px; 
                        background-color: #f9f9f9; 
                    }}
                    h1 {{ 
                        text-align: center; 
                        color: #007bff; 
                        font-weight: 700; 
                    }}
                    table {{ 
                        width: 100%; 
                        border-collapse: collapse; 
                        margin-top: 20px; 
                        box-shadow: 0 2px 5px rgba(0,0,0,0.1); 
                        table-layout: fixed;
                        border-collapse: collapse;
                    }}
                    th, td {{ 
                        border: 1px solid #ddd; 
                        padding: 10px; 
                        text-align: left; 
                        word-wrap: break-word;
                    }}
                    th {{ 
                        background-color: #007bff; 
                        color: white; 
                        font-weight: 700; 
                    }}
                    tr:nth-child(even) {{ 
                        background-color: #f2f2f2; 
                    }}
                    .highlight {{ 
        font-weight: bold; 
        color: #e91e63; 
    }} 
    .header_1 {{
        font-size: 18px;
        font-weight: 700;
        margin-bottom: 5px;
    }}
    .header_2 {{
        font-size: 16px;
        font-weight: 700;
    }}
    .stt {{
        text-align: center;
    }}
    .row-item-container {{
        display: flex;
    }}
    .image-grid {{
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 10px;
        margin-top: 20px;
    }}
    .image-grid img {{
        width: 100%;
        height: auto;
        border: 1px solid #ddd;
        border-radius: 5px;
    }}
    .two-columns {{
        display: flex;
        gap: 20px;
        margin-top: 20px;
    }}
    .column {{
        flex: 1;
    }}
    .row-item-container {{
        display: flex;
        justify-content: flex-start;
        gap: 10px;
        margin-bottom: 8px;
    }}
    .row-item-container div:first-child {{
        font-weight: bold;
    }}
                </style>
            </head>
            <body>
    <div class=""container"">
        <div class=""header"">
            <div class=""header_1"">CÔNG TY TNHH PRIMEWOOD CORPORATION (VIỆT NAM)</div>
            <div class=""header_2"">Địa chỉ: Khu Kinh Tế Vũng Áng, Kỳ Lợi, Kỳ Anh, Hà Tĩnh</div>
        </div>
        <h1>PHIẾU CÂN XE</h1>
        <div class=""stt"">STT: NH-39</div>
        <div class=""two-columns"">
            <!-- Cột trái -->
            <div class=""column"">
                <div class=""row-item-container"">
                    <div>- Hàng hóa :</div> <div>Dăm gỗ</div>
                </div>
                <div class=""row-item-container"">
                    <div>- Khách hàng :</div> <div>Công Ty TNHH Chế Biến Lâm Sản An Phước</div>
                </div>
                <div class=""row-item-container"">
                    <div>- Địa chỉ :</div> <div>Vũng Áng, Kỳ Lợi, Kỳ Anh</div>
                </div>
                <div class=""row-item-container"">
                    <div>- Biển số xe :</div> <div>37H55055</div>
                </div>
                <div class=""row-item-container"">
                    <div>- Ghi chú :</div> <div>Vũng Áng, Kỳ Lợi, Kỳ Anh</div>
                </div>
            </div>

            <!-- Cột phải -->
            <div class=""column"">
                <div class=""row-item-container"">
                    <div>- Số điện thoại :</div> <div>0912 345 678</div>
                </div>
                <div class=""row-item-container"">
                    <div>- Kiểu cân :</div> <div>Cân nhập hàng</div>
                </div>
            </div>
        </div>
        <table>
            <thead>
                <tr>
                    <th>TL xe vào</th>
                    <th>TL xe ra</th>
                    <th>TL hàng</th>
                    <th>Ngày giờ cân vào</th>
                    <th>Ngày giờ cân ra</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>12,000 kg</td>
                    <td>6,500 kg</td>
                    <td class=""highlight"">5,500 kg</td>
                    <td>28/08/2025 08:30</td>
                    <td>28/08/2025 10:15</td>
                </tr>
            </tbody>
        </table>
        <div>
            <div>Ảnh chụp xe</div>
            <div class=""image-grid"">
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 1"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 2"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 3"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 4"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 5"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 6"" />
            </div>
        </div>
    </div>
</body>
            </html>";

            var tempHtmlPath = Path.Combine(Path.GetTempPath(), "temp.html");
            await File.WriteAllTextAsync(tempHtmlPath, htmlContent, Encoding.UTF8);

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync($"file://{tempHtmlPath}", new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
            // Corrected PdfAsync call: Use PdfOptions directly to get byte[]
            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = pageSize == "A4" ? PuppeteerSharp.Media.PaperFormat.A4 : PuppeteerSharp.Media.PaperFormat.A5,
                PrintBackground = true,
                MarginOptions = new PuppeteerSharp.Media.MarginOptions
                {
                    Top = "0mm",
                    Bottom = "0mm",
                    Left = "0mm",
                    Right = "0mm"
                }
            });

            return pdfBytes;
        }
    }
}
