using ClosedXML.Excel;
using CMS_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025.Core.Services.ShareServices
{ 
    public class ExcelService
    {
        public async Task<string> ExportVehicleToDownloadsAsync(List<TblVehicle> vehicles)
        {
            // 📂 Lấy thư mục Downloads (Windows)
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            // 📄 Tạo tên file
            var fileName = $"Danhsachxe_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(downloadsPath, fileName);

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Vehicles");

                // 📝 Header
                ws.Cell(1, 1).Value = "ID";
                ws.Cell(1, 2).Value = "Biển số xe";
                ws.Cell(1, 3).Value = "Mã thẻ RFID";
                ws.Cell(1, 4).Value = "Tên lái xe";
                ws.Cell(1, 5).Value = "SĐT lái xe";
                ws.Cell(1, 6).Value = "Khối lượng không tải";
                ws.Cell(1, 7).Value = "Công ty vận chuyển"; 

                // 📌 Format header
                var headerRange = ws.Range(1, 1, 1, 12);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // 📊 Đổ dữ liệu
                int row = 2;
                foreach (var v in vehicles)
                {
                    ws.Cell(row, 1).Value = v.Id;
                    ws.Cell(row, 2).Value = v.VehiceCode;
                    ws.Cell(row, 3).Value = v.RfidCode;
                    ws.Cell(row, 4).Value = v.DriverName;
                    ws.Cell(row, 5).Value = v.DriverPhone;
                    ws.Cell(row, 6).Value = v.TonnageDefault;
                    ws.Cell(row, 7).Value = v.TransportationCompany; 

                    row++;
                }

                // 📐 Auto fit
                ws.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }

            return filePath;
        }
        public async Task<List<TblVehicle>> ImportVehicleFromExcelAsync(string filePath)
        {
            var vehicles = new List<TblVehicle>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var ws = workbook.Worksheet(1); // Lấy sheet đầu tiên
                var rows = ws.RangeUsed().RowsUsed().Skip(1); // Bỏ header (hàng 1)

                foreach (var row in rows)
                {
                    var vehicle = new TblVehicle
                    {
                        Id = row.Cell(1).GetValue<int>(),
                        VehiceCode = row.Cell(2).GetString(),
                        RfidCode = row.Cell(3).GetString(),
                        DriverName = row.Cell(4).GetString(),
                        DriverPhone = row.Cell(5).GetString(),
                        TonnageDefault = row.Cell(6).GetValue<double?>(),
                        TransportationCompany = row.Cell(7).GetString(),
                        //Status = ParseStatus(row.Cell(8).GetString()),
                        //CreateDay = ParseDate(row.Cell(9).GetString()),
                        //CreateBy = row.Cell(10).GetString(),
                        //UpdateDay = ParseDate(row.Cell(11).GetString()),
                        //UpdateBy = row.Cell(12).GetString(),
                    };

                    vehicles.Add(vehicle);
                }
            }

            return vehicles;
        }

        public async Task<string> ExportProductToDownloadsAsync(List<TblProduct> products)
        {
            // 📂 Lấy thư mục Downloads (Windows)
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            // 📄 Tạo tên file
            var fileName = $"Danhsachsanpham_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(downloadsPath, fileName);

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("products");

                // 📝 Header
                ws.Cell(1, 1).Value = "Mã sản phẩm";
                ws.Cell(1, 2).Value = "Tên sản phẩm";
                ws.Cell(1, 3).Value = "Đơn vị";
                ws.Cell(1, 4).Value = "Tỉ lệ";
                ws.Cell(1, 5).Value = "Giá nhập";
                ws.Cell(1, 6).Value = "Giá bán";

                // 📌 Format header
                var headerRange = ws.Range(1, 1, 1, 12);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // 📊 Đổ dữ liệu
                int row = 2;
                foreach (var v in products)
                {
                    ws.Cell(row, 1).Value = v.Code;
                    ws.Cell(row, 2).Value = v.Name;
                    ws.Cell(row, 3).Value = v.UnitName;
                    ws.Cell(row, 4).Value = v.Proportion;
                    ws.Cell(row, 5).Value = v.PriceInput;
                    ws.Cell(row, 6).Value = v.PriceOutput;

                    row++;
                }

                // 📐 Auto fit
                ws.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }

            return filePath;
        }
        public async Task<string> ExportCustomerToDownloadsAsync(List<TblCustomer> customers)
        {
            // 📂 Lấy thư mục Downloads (Windows)
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            // 📄 Tạo tên file
            var fileName = $"Danhsachkhachhang_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(downloadsPath, fileName);

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("customers");

                // 📝 Header
                ws.Cell(1, 1).Value = "Mã KH";
                ws.Cell(1, 2).Value = "Tên KH";
                ws.Cell(1, 3).Value = "Điện thoại";
                ws.Cell(1, 4).Value = "Email";
                ws.Cell(1, 5).Value = "Địa chỉ";
                ws.Cell(1, 6).Value = "Ghi chú";

                // 📌 Format header
                var headerRange = ws.Range(1, 1, 1, 12);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // 📊 Đổ dữ liệu
                int row = 2;
                foreach (var v in customers)
                {
                    ws.Cell(row, 1).Value = v.Code;
                    ws.Cell(row, 2).Value = v.Name;
                    ws.Cell(row, 3).Value = v.Phone;
                    ws.Cell(row, 4).Value = v.Email;
                    ws.Cell(row, 5).Value = v.Address;
                    ws.Cell(row, 6).Value = v.Note;

                    row++;
                }

                // 📐 Auto fit
                ws.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }

            return filePath;
        }
        //ExportReportScaleToDownloadsAsync
        public async Task<string> ExportReportScaleToDownloadsAsync(List<TblScale> scales)
        {
            // 📂 Lấy thư mục Downloads (Windows)
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            // 📄 Tạo tên file
            var fileName = $"Baocaocanhang_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(downloadsPath, fileName);

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Cân hàng");

                // 📝 Header
                ws.Cell(1, 1).Value = "STT";
                ws.Cell(1, 2).Value = "Ngày cân";
                ws.Cell(1, 3).Value = "Biển số xe";
                ws.Cell(1, 4).Value = "Khách hàng";
                ws.Cell(1, 5).Value = "Địa chỉ";
                ws.Cell(1, 6).Value = "Hàng hóa";
                ws.Cell(1, 7).Value = "Giá bán";
                ws.Cell(1, 8).Value = "Cân lần 1";
                ws.Cell(1, 9).Value = "KL cân lần 1";
                ws.Cell(1, 10).Value = "Cân lần 2";
                ws.Cell(1, 11).Value = "KL cân lần 2";
                ws.Cell(1, 12).Value = "KL hàng";
                ws.Cell(1, 13).Value = "Thành tiền";

                // 📌 Format header
                var headerRange = ws.Range(1, 1, 1, 12);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // 📊 Đổ dữ liệu
                int row = 2;
                var stt = 0;
                foreach (var v in scales)
                {
                    stt++;
                    ws.Cell(row, 1).Value = stt;
                    ws.Cell(row, 2).Value = v.CreateDay?.ToString("dd-MM-yyyy HH:mm");
                    ws.Cell(row, 3).Value = v.Vehicle;
                    ws.Cell(row, 4).Value = v.CustomerName;
                    ws.Cell(row, 5).Value = v.CustomerAddress;
                    ws.Cell(row, 6).Value = v.ProductName;
                    ws.Cell(row, 7).Value = v.ProductPrice;
                    ws.Cell(row, 8).Value = v.TimeIn?.ToString("dd-MM-yyyy HH:mm");
                    ws.Cell(row, 9).Value = v.WeightIn;
                    ws.Cell(row, 10).Value = v.TimeOut?.ToString("dd-MM-yyyy HH:mm");
                    ws.Cell(row, 12).Value = v.WeightOut;
                    ws.Cell(row, 12).Value = v.ProductNumber;
                    ws.Cell(row, 13).Value = v.TotalMoney;


                    row++;
                }

                // 📐 Auto fit
                ws.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }

            return filePath;
        }

        //private bool? ParseStatus(string status)
        //{
        //    if (string.IsNullOrWhiteSpace(status)) return null;
        //    if (status.Equals("Active", StringComparison.OrdinalIgnoreCase)) return true;
        //    if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)) return false;
        //    return null;
        //}

        //private DateTime? ParseDate(string input)
        //{
        //    if (DateTime.TryParse(input, out var dt))
        //        return dt;
        //    return null;
        //}
    }
}
