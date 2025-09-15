using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WinFormsAppTestScale
{
    public enum ScaleProtocol
    {
        Unknown = 0,
        LineSimple = 1,       // Chuỗi kết thúc bằng CRLF
        StxOnly = 2,          // Có STX, không có ETX (dùng độ dài tối thiểu)
        STXETX = 3,           // Có STX và ETX
        LineWithSTAndKg = 4,  // Frame chứa ST...kg
        STXColon = 5          // Có STX và kết thúc bằng dấu ':'
    }
    public static class ScaleParser
    {
        private static string dataPlus = "";
        private static ScaleProtocol? detected = null; // null = chưa biết

        public static List<double> ExtractWeights(ref StringBuilder buffer, ref ScaleProtocol protocol)
        {
            var results = new List<double>();
            string content = buffer.ToString();
            buffer.Clear();

            // Nếu chưa detect → thử auto-detect dựa trên content
            if (protocol == 0)
            {
                protocol = DetectProtocol(content);
                if (protocol != 0)
                {
                    Console.WriteLine($"[AutoDetect] Protocol = {protocol}");
                }
            }

            switch (protocol)
            {
                case ScaleProtocol.LineSimple:
                    while (content.Contains("\r\n"))
                    {
                        int idx = content.IndexOf("\r\n", StringComparison.Ordinal);
                        string line = content.Substring(0, idx).Trim();
                        content = content.Substring(idx + 2);

                        if (TryParseLine(line, out double w))
                            results.Add(w);
                    }
                    buffer.Append(content); // phần còn dư chưa đủ CRLF
                    break;

                case ScaleProtocol.StxOnly:
                    {
                        int stIndex = content.IndexOf('\u0002'); // STX
                        if (stIndex >= 0)
                        {
                            dataPlus = content.Substring(stIndex + 1);
                            if (dataPlus.Length > 5 && TryParseLine(dataPlus, out double w))
                            {
                                results.Add(w);
                                dataPlus = "";
                            }
                        }
                        else
                        {
                            dataPlus += content;
                            if (dataPlus.Length > 5 && TryParseLine(dataPlus, out double w))
                            {
                                results.Add(w);
                                dataPlus = "";
                            }
                        }
                    }
                    break;

                case ScaleProtocol.STXETX:
                    foreach (char c in content)
                    {
                        if (c == '\u0002') // STX
                            dataPlus = "";
                        else if (c == '\u0003') // ETX
                        {
                            if (TryParseLine(dataPlus, out double w))
                                results.Add(w);
                            dataPlus = "";
                        }
                        else
                            dataPlus += c;
                    }
                    break;

                case ScaleProtocol.LineWithSTAndKg:
                    dataPlus += content;

                    while (true)
                    {
                        // Tìm ST đầu tiên
                        int idxSt = dataPlus.IndexOf("ST", StringComparison.OrdinalIgnoreCase);

                        if (idxSt < 0)
                        {
                            // Nếu không có ST, nhưng có "kg" thì có thể là fragment bắt đầu từ giữa frame
                            int idxKgOnly = dataPlus.IndexOf("kg", StringComparison.OrdinalIgnoreCase);
                            if (idxKgOnly >= 0)
                            {
                                // xử lý phần trước "kg" như 1 frame (dù thiếu "ST"), tránh kẹt buffer
                                string frame = dataPlus.Substring(0, idxKgOnly + 2);
                                if (TryParseLine(frame, out double w))
                                    results.Add(w);

                                dataPlus = dataPlus.Substring(idxKgOnly + 2);
                                continue;
                            }

                            // không có ST và không có kg -> chờ thêm dữ liệu
                            break;
                        }

                        // Có ST, tìm "kg" **sau** ST
                        int idxKgAfterSt = dataPlus.IndexOf("kg", idxSt, StringComparison.OrdinalIgnoreCase);

                        if (idxKgAfterSt > idxSt)
                        {
                            // Tìm được 1 frame hoàn chỉnh ST ... kg
                            string frame = dataPlus.Substring(idxSt, idxKgAfterSt - idxSt + 2);
                            if (TryParseLine(frame, out double w))
                                results.Add(w);
                            // bỏ phần đã xử lý, tiếp tục lặp để tách frame kế tiếp
                            dataPlus = dataPlus.Substring(idxKgAfterSt + 2);
                            continue;
                        }

                        // Trường hợp: có ST nhưng chưa có kg sau đó => chưa đủ dữ liệu, chờ lần sau
                        // Tuy nhiên có thể tồn tại một "kg" trước ST (đã bỏ qua), xử lý để không kẹt
                        int idxKgBefore = dataPlus.IndexOf("kg", StringComparison.OrdinalIgnoreCase);
                        if (idxKgBefore >= 0 && idxKgBefore < idxSt)
                        {
                            // xử lý phần trước kg (nếu có số) rồi tiếp tục
                            string frame = dataPlus.Substring(0, idxKgBefore + 2);
                            if (TryParseLine(frame, out double w))
                                results.Add(w);
                            dataPlus = dataPlus.Substring(idxKgBefore + 2);
                            continue;
                        }

                        // Chưa đủ để tạo frame hoàn chỉnh (ST có nhưng kg chưa có) -> thoát chờ dữ liệu tiếp
                        break;
                    }
                    break;

                case ScaleProtocol.STXColon:
                    foreach (char c in content)
                    {
                        if (c == '\u0002') // STX
                            dataPlus = "";
                        else if (c == ':') // kết thúc frame
                        {
                            if (TryParseLine(dataPlus, out double w))
                                results.Add(w);
                            dataPlus = "";
                        }
                        else
                            dataPlus += c;
                    }
                    break;
                case ScaleProtocol.Unknown:
                    // Bắt số dạng 12.34 hoặc 1234
                    {
                        var p = DetectProtocol(content);
                        if (p != ScaleProtocol.Unknown)
                        {
                            protocol = p;
                            Console.WriteLine($"[AutoDetect] Protocol = {protocol}");
                        }
                        else
                        {
                            // fallback: thử parse thẳng số luôn, đừng bỏ qua
                            if (Regex.Match(content, @"[-+]?\d+(\.\d+)?") is Match m && m.Success)
                            {
                                if (TryParseLine(m.Value, out double w))
                                    results.Add(w);
                            }

                            // giữ lại dữ liệu để lần sau detect tiếp
                            buffer.Append(content);
                            return results;
                        }
                    
                    }
                    break;
            }

            return results;
        }

        private static ScaleProtocol DetectProtocol(string sample)
        {
            if (string.IsNullOrEmpty(sample))
                return ScaleProtocol.Unknown;

            if (sample.Contains("\r\n"))
                return ScaleProtocol.LineSimple;

            if (sample.Contains("\u0002") && sample.Contains("\u0003"))
                return ScaleProtocol.STXETX;

            if (sample.Contains("\u0002") && sample.Contains(":"))
                return ScaleProtocol.STXColon;

            if (sample.Contains("ST") && sample.Contains("kg"))
                return ScaleProtocol.LineWithSTAndKg;

            if (sample.Contains("\u0002"))
                return ScaleProtocol.StxOnly;

            return ScaleProtocol.Unknown;
        }
        private static bool TryParseLine(string frame, out double weight)
        {
            weight = 0;

            // Tìm "kg"
            int idxKg = frame.IndexOf("kg", StringComparison.OrdinalIgnoreCase);
            if (idxKg <= 0) return false;

            // Lấy phần trước "kg"
            string beforeKg = frame.Substring(0, idxKg);

            // Tìm số (chỉ lấy số cuối cùng trong chuỗi)
            var matches = System.Text.RegularExpressions.Regex.Matches(beforeKg, @"\d+");
            if (matches.Count == 0) return false;

            string numberStr = matches[matches.Count - 1].Value;
            if (double.TryParse(numberStr, out weight))
                return true;

            return false;
        }

        private static bool TryParseLine_bak(string line, out double w)
        {
            w = -1;
            var digits = new string(line.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            if (double.TryParse(digits.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out w))
                return true;

            return false;
        }
    }
}
