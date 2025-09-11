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

                    if (dataPlus.Contains("ST") && dataPlus.Contains("kg"))
                    {
                        string frame = dataPlus;
                        if (TryParseLine(frame, out double w))
                            results.Add(w);

                        dataPlus = "";
                    }
                    else if (dataPlus.EndsWith("kg"))
                    {
                        if (TryParseLine(dataPlus, out double w))
                            results.Add(w);
                        dataPlus = "";
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
            }

            return results;
        }

        private static ScaleProtocol DetectProtocol(string sample)
        {
            if (string.IsNullOrEmpty(sample))
                return 0;

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

            return 0; // chưa chắc chắn
        }

        private static bool TryParseLine(string line, out double w)
        {
            w = -1;
            var digits = new string(line.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            if (double.TryParse(digits.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out w))
                return true;

            return false;
        }
    }
}
