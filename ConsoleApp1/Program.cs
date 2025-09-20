using System.IO.Ports;
using System.Text;

namespace ConsoleApp1
{
    using System;
    using System.IO.Ports;
    using System.Text;
    using System.Threading;

    using System;
    using System.IO.Ports;
    using System.Text;
    using System.Threading;

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Scale Simulator ===");
            Console.Write("Nhập COM port (ví dụ COM2): ");
            string portName = Console.ReadLine()?.Trim() ?? "COM2";

            Console.Write("Chọn protocol (1=Line, 2=STX-only, 3=STX+ETX, 4=ST/kg, 5=STX+:, 6=STXETX fixed): ");
            string mode = Console.ReadLine()?.Trim() ?? "6";

            Console.Write("Noise mode (0=nguyên frame, 1=chunk như ReadExisting): ");
            string noiseMode = Console.ReadLine()?.Trim() ?? "0";

            Console.Write("Kiểu tạo số cân (0=random, 1=triangle wave): ");
            string modeWeight = Console.ReadLine()?.Trim() ?? "0";

            Console.WriteLine($"Đang mở {portName} với protocol={mode}, noiseMode={noiseMode}, weightMode={modeWeight} ...");

            using (var sp = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One))
            {
                sp.Encoding = Encoding.ASCII;
                sp.Open();

                var rand = new Random();

                int weight = 11000;
                bool goingUp = true; // cho triangle wave

                while (true)
                {
                    // ==== Chọn số cân ====
                    if (modeWeight == "0") // random
                    {
                        weight = rand.Next(11000, 68001);
                    }
                    else // triangle wave
                    {
                        if (goingUp)
                        {
                            weight += 100;
                            if (weight >= 68000) goingUp = false;
                        }
                        else
                        {
                            weight -= 100;
                            if (weight <= 11000) goingUp = true;
                        }
                    }

                    string frame = BuildFrame(weight, mode);

                    if (noiseMode == "0")
                    {
                        // Gửi nguyên frame, nối 5 lần như thực tế
                        string full = string.Concat(Enumerable.Repeat(frame, 5));
                        sp.Write(full);
                        Console.WriteLine($"[SEND] {full.Replace("\r", "\\r").Replace("\n", "\\n")}");
                    }
                    else
                    {
                        // Gửi frame theo chunk noise (mô phỏng ReadExisting)
                        string full = string.Concat(Enumerable.Repeat(frame, 5));
                        SendFrameWithNoise(sp, full, rand);
                    }

                    Thread.Sleep(200); // tốc độ gửi (ms)
                }
            }
        }

        private static string BuildFrame(int weight, string mode)
        {
            switch (mode)
            {
                case "1": // CRLF
                    return $"{weight} kg\r\n";

                case "2": // STX only
                    return "\u0002" + $"{weight} kg";

                case "3": // STX ... ETX
                    return "\u0002" + $"{weight} kg" + "\u0003";

                case "4": // ST ... kg
                    return $"ST,GS,+ {weight}kg";

                case "5": // STX + colon
                    return "\u0002" + $"{weight}:kg";

                case "6": // STXETX fixed length: 1 sign + 6 số nguyên + 2 số thập phân + flag
                    string val = weight.ToString("D6"); // 6 chữ số
                    string decimalPart = "01";          // giả lập phần thập phân
                    string flag = "E";                  // có thể thay B, S, F, tùy trạng thái
                    return $"\u0002+{val}{decimalPart}{flag}\u0003";

                default:
                    return $"{weight} kg\r\n";
            }
        }

        private static void SendFrameWithNoise(SerialPort sp, string frame, Random rand)
        {
            int index = 0;
            while (index < frame.Length)
            {
                // Mỗi lần gửi 1–4 ký tự để mô phỏng ReadExisting không đều
                int chunkSize = rand.Next(1, 5);
                if (index + chunkSize > frame.Length)
                    chunkSize = frame.Length - index;

                string chunk = frame.Substring(index, chunkSize);
                sp.Write(chunk);
                Console.Write($"[CHUNK] \"{chunk.Replace("\r", "\\r").Replace("\n", "\\n")}\"");

                index += chunkSize;

                // Ngẫu nhiên delay để dính nhiều frame vào cùng 1 lần ReadExisting
                if (rand.NextDouble() < 0.3)
                    Thread.Sleep(rand.Next(5, 20));
            }

            Console.WriteLine($"  => [FRAME DONE] \"{frame.Replace("\r", "\\r").Replace("\n", "\\n")}\"");
        }
    }
}
