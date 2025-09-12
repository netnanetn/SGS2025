using System.IO.Ports;
using System.Text;

namespace ConsoleApp1
{
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

            Console.Write("Chọn protocol (1=Line, 2=STX-only, 3=STX+ETX, 4=ST/kg, 5=STX+:): ");
            string mode = Console.ReadLine()?.Trim() ?? "1";

            Console.Write("Gửi kiểu (0=nguyên frame, 1=chunk từng phần): ");
            string sendMode = Console.ReadLine()?.Trim() ?? "0";

            Console.Write("Kiểu tạo số cân (0=random, 1=triangle wave): ");
            string modeWeight = Console.ReadLine()?.Trim() ?? "0";

            Console.WriteLine($"Đang mở {portName} với protocol={mode}, sendMode={sendMode}, weightMode={modeWeight} ...");

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

                    if (sendMode == "0")
                    {
                        // Gửi nguyên frame
                        sp.Write(frame);
                        Console.WriteLine($"[SEND] {frame.Replace("\r", "\\r").Replace("\n", "\\n")}");
                    }
                    else
                    {
                        // Gửi từng chunk nhỏ
                        int index = 0;
                        while (index < frame.Length)
                        {
                            int chunkSize = rand.Next(1, 4); // 1–3 ký tự/lần
                            if (index + chunkSize > frame.Length)
                                chunkSize = frame.Length - index;

                            string chunk = frame.Substring(index, chunkSize);
                            sp.Write(chunk);

                            Console.WriteLine($"[CHUNK] \"{chunk.Replace("\r", "\\r").Replace("\n", "\\n")}\"");

                            index += chunkSize;
                            Thread.Sleep(rand.Next(5, 30)); // delay ngẫu nhiên
                        }
                        Console.WriteLine($"[FRAME DONE] \"{frame.Replace("\r", "\\r").Replace("\n", "\\n")}\"");
                    }

                    Thread.Sleep(200); // tốc độ gửi (ms)
                }
            }
        }

        private static string BuildFrame(double weight, string mode)
        {
            string val = weight.ToString();

            switch (mode)
            {
                case "1": // CRLF
                    return $"{val} kg\r\n";

                case "2": // STX only (0x02)
                    return "\u0002" + $"{val} kg";

                case "3": // STX ... ETX
                    return "\u0002" + $"{val} kg" + "\u0003";

                case "4": // ST ... kg
                    return $"ST,{val} kg";

                case "5": // STX + colon
                    return "\u0002" + $"{val}:kg";

                default:
                    return $"{val} kg\r\n";
            }
        }
    }
}
