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
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <title>Xem thử phiếu in</title>
    <link href=""https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap"" rel=""stylesheet"">
    <style>
        @page {{
            size: A4;
            margin: 10mm;
        }}

        body {{
            font-family: 'Roboto', Arial, sans-serif;
            font-size: 12pt;
            color: #333;
        }}

        .container {{
            width: 170mm;
            margin: 0 auto;
            padding: 10mm;
        }}

        h1 {{
            text-align: center;
            font-weight: 800;
            font-size: 28px;
        }}

        table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 10mm;
            table-layout: fixed;
        }}

        th, td {{
            border: 1px solid #ddd;
            padding: 5mm;
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
            font-weight: 700;
            color: black;
            font-size: 16px;
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
        .stt{{
            text-align: center;
            font-weight: 700;
        }}
        .row-item-container{{
            display: flex;
        }}
        .image-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
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
                width: 120px;
            }}
        .info-table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
        }}

            .info-table th,
            .info-table td {{
                padding: 6px 10px;
                border: 1px solid #ddd;
                text-align: left;
                vertical-align: top;
            }}

            .info-table th {{
                width: 120px;
                background-color: #f2f2f2;
                font-weight: bold;
                color: black;
            }}

        .signature-1{{
            font-weight: 700;
        }}

        .signature-section {{
            display: flex;
            justify-content: space-between;
            margin-top: 50px;
            text-align: center;
        }}

        .signature-col {{
            flex: 1;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            height: 150px;
        }}

            .signature-col div:first-child {{
                font-weight: bold;
            }}

        .signature-placeholder {{
            font-style: italic;
            font-size: 12px;
        }}
        .scale-table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
        }}

            .scale-table th,
            .scale-table td {{
                border: 1px solid #ddd;
                padding: 8px;
                text-align: center;
            }}

                .scale-table th.short {{
                    width: 15%;
                }}

                .scale-table th.long {{
                    width: 26%;
                }}

            .scale-table th {{
                background-color: #007bff;
                color: black;
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
        <div class="""">
            <table class=""info-table"">
                <tr>
                    <th>Hàng hóa</th>
                    <td>Dăm gỗ</td>
                </tr>
                <tr>
                    <th>Khách hàng</th>
                    <td>Công Ty TNHH Chế Biến Lâm Sản An Phước</td>
                </tr>
                <tr>
                    <th>Địa chỉ</th>
                    <td>Vũng Áng, Kỳ Lợi, Kỳ Anh</td>
                </tr>
                <tr>
                    <th>Biển số xe</th>
                    <td>37H55055</td>
                </tr>
                <tr>
                    <th>Ghi chú</th>
                    <td>Vũng Áng, Kỳ Lợi, Kỳ Anh</td>
                </tr>
                <tr>
                    <th>Số điện thoại</th>
                    <td>0912 345 678</td>
                </tr>
                <tr>
                    <th>Kiểu cân</th>
                    <td>Cân nhập hàng</td>
                </tr>
            </table>
        </div>
        <table class=""scale-table"">
            <thead>
                <tr>
                    <th class=""short"">TL xe vào</th>
                    <th class=""short"">TL xe ra</th>
                    <th class=""short"">TL hàng</th>
                    <th class=""long"">Ngày giờ cân vào</th>
                    <th class=""long"">Ngày giờ cân ra</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td class=""highlight"">12,000 kg</td>
                    <td class=""highlight"">6,500 kg</td>
                    <td class=""highlight"">5,500 kg</td>
                    <td>28/08/2025 08:30</td>
                    <td>28/08/2025 10:15</td>
                </tr>
            </tbody>
        </table>
        <div>
            <div class=""image-grid"">
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 1"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 2"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 3"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 4"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 5"" />
                <img src=""https://trungvietsoftware-cdn.cdn.vccloud.vn/tramda/tvs/tvs_1756457126_113236022.jpeg"" alt=""Ảnh 6"" />
            </div>
        </div>
        <div class=""signature-section"">
            <div class=""signature-col"">
                <div class=""signature-1"">Nhân viên bàn cân</div>
                <div class=""signature-placeholder"">(Ký và ghi rõ họ tên)</div>
            </div>
            <div class=""signature-col"">
                <div class=""signature-1"">Khách hàng</div>
                <div class=""signature-placeholder"">(Ký và ghi rõ họ tên)</div>
            </div>
            <div class=""signature-col"">
                <div class=""signature-1"">Công ty TNHH Primewood</div>
                <div class=""signature-placeholder"">(Ký và ghi rõ họ tên)</div>
            </div>
        </div>
    </div>
</body>
</html>
";

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
