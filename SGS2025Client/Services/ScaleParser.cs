using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Services
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

            // Nếu chưa detect protocol -> thử auto-detect
            if (protocol == ScaleProtocol.Unknown)
            {
                protocol = DetectProtocol(content);
                if (protocol != ScaleProtocol.Unknown)
                    Console.WriteLine($"[AutoDetect] Protocol = {protocol}");
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
                    buffer.Append(content); // giữ phần dư chưa đủ CRLF
                    break;

                case ScaleProtocol.StxOnly:
                    dataPlus += content;
                    while (true)
                    {
                        int stIndex = dataPlus.IndexOf('\u0002'); // STX
                        if (stIndex < 0)
                            break;

                        string tmp = dataPlus.Substring(stIndex + 1); // bỏ STX
                        if (tmp.Length > 5 && TryParseLine(tmp, out double w))
                        {
                            results.Add(w);
                            dataPlus = "";
                        }
                        else
                        {
                            dataPlus = tmp; // giữ fragment
                            break;
                        }
                    }
                    break;

                case ScaleProtocol.STXETX:
                    dataPlus += content;
                    while (true)
                    {
                        int stIndex = dataPlus.IndexOf('\u0002');
                        int etIndex = dataPlus.IndexOf('\u0003');

                        if (stIndex >= 0 && etIndex > stIndex)
                        {
                            string frame = dataPlus.Substring(stIndex + 1, etIndex - stIndex - 1);
                            if (TryParseLine_STXETX(frame, out double w))
                                results.Add(w);

                            dataPlus = dataPlus.Substring(etIndex + 1);
                        }
                        else if (etIndex >= 0 && (stIndex < 0 || etIndex < stIndex))
                        {
                            // ETX trước STX -> loại bỏ rác
                            dataPlus = dataPlus.Substring(etIndex + 1);
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;

                case ScaleProtocol.LineWithSTAndKg:
                    dataPlus += content;
                    while (true)
                    {
                        int idxSt = dataPlus.IndexOf("ST", StringComparison.OrdinalIgnoreCase);
                        if (idxSt < 0)
                        {
                            int idxKgOnly = dataPlus.IndexOf("kg", StringComparison.OrdinalIgnoreCase);
                            if (idxKgOnly >= 0)
                            {
                                string frame = dataPlus.Substring(0, idxKgOnly + 2);
                                if (TryParseLine_STKG(frame, out double w))
                                    results.Add(w);

                                dataPlus = dataPlus.Substring(idxKgOnly + 2);
                                continue;
                            }
                            break;
                        }

                        int idxKgAfterSt = dataPlus.IndexOf("kg", idxSt, StringComparison.OrdinalIgnoreCase);
                        if (idxKgAfterSt > idxSt)
                        {
                            string frame = dataPlus.Substring(idxSt, idxKgAfterSt - idxSt + 2);
                            if (TryParseLine_STKG(frame, out double w))
                                results.Add(w);

                            dataPlus = dataPlus.Substring(idxKgAfterSt + 2);
                            continue;
                        }

                        int idxKgBefore = dataPlus.IndexOf("kg", StringComparison.OrdinalIgnoreCase);
                        if (idxKgBefore >= 0 && idxKgBefore < idxSt)
                        {
                            string frame = dataPlus.Substring(0, idxKgBefore + 2);
                            if (TryParseLine_STKG(frame, out double w))
                                results.Add(w);

                            dataPlus = dataPlus.Substring(idxKgBefore + 2);
                            continue;
                        }

                        break;
                    }
                    break;

                case ScaleProtocol.STXColon:
                    foreach (char c in content)
                    {
                        if (c == '\u0002')
                            dataPlus = "";
                        else if (c == ':')
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
                default:
                    buffer.Append(content); // chưa biết protocol, giữ lại
                    break;
            }

            return results;
        }

        private static bool TryParseLine(string line, out double weight)
        {
            weight = 0;
            line = line.Trim().Replace("kg", "").Replace("+", "").Replace(",", "").Trim();
            return double.TryParse(line, out weight);
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
        private static bool TryParseLine_STKG(string frame, out double weight)
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

        private static bool TryParseLine_STXETX(string line, out double w)
        {
            w = -1;
            var digits = new string(line.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            if (double.TryParse(digits.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out w))
                return true;

            return false;
        }
    }
}
