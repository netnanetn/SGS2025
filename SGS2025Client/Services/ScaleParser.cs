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
        LineSimple = 1,       // Chuỗi kết thúc bằng CRLF
        StxOnly = 2,          // Có STX, không có ETX (dùng độ dài tối thiểu)
        STXETX = 3,           // Có STX và ETX
        LineWithSTAndKg = 4,  // Frame chứa ST...kg
        STXColon = 5          // Có STX và kết thúc bằng dấu ':'
    }
    public static class ScaleParser
    {
        // Buffer phụ trợ để ghép frame khi nhiều lần mới đủ
        private static string dataPlus = "";

        public static List<double> ExtractWeights(ref StringBuilder buffer, ScaleProtocol protocol)
        {
            var results = new List<double>();
            string content = buffer.ToString();
            buffer.Clear();

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
                            dataPlus = content.Substring(stIndex + 1); // bỏ STX
                                                                       // Tạm coi >5 ký tự là đủ 1 frame
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
                    {
                        foreach (char c in content)
                        {
                            if (c == '\u0002') // STX
                            {
                                dataPlus = "";
                            }
                            else if (c == '\u0003') // ETX
                            {
                                if (TryParseLine(dataPlus, out double w))
                                    results.Add(w);
                                dataPlus = "";
                            }
                            else
                            {
                                dataPlus += c;
                            }
                        }
                    }
                    break;

                case ScaleProtocol.LineWithSTAndKg:
                    {
                        dataPlus += content;

                        if (dataPlus.Contains("ST") && dataPlus.Contains("kg"))
                        {
                            string frame = dataPlus;
                            if (TryParseLine(frame, out double w))
                                results.Add(w);

                            dataPlus = ""; // reset
                        }
                        else if (dataPlus.EndsWith("kg"))
                        {
                            if (TryParseLine(dataPlus, out double w))
                                results.Add(w);
                            dataPlus = "";
                        }
                    }
                    break;

                case ScaleProtocol.STXColon:
                    {
                        foreach (char c in content)
                        {
                            if (c == '\u0002') // STX
                            {
                                dataPlus = "";
                            }
                            else if (c == ':') // kết thúc
                            {
                                if (TryParseLine(dataPlus, out double w))
                                    results.Add(w);
                                dataPlus = "";
                            }
                            else
                            {
                                dataPlus += c;
                            }
                        }
                    }
                    break;
            }

            return results;
        }

        private static bool TryParseLine(string line, out double weight)
        {
            weight = -1;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            // Lọc ký tự số, dấu . và -
            var sb = new StringBuilder();
            foreach (char c in line)
            {
                if (char.IsDigit(c) || c == '.' || c == '-')
                    sb.Append(c);
            }

            if (double.TryParse(sb.ToString(), out weight))
                return true;

            return false;
        }
    }
}
