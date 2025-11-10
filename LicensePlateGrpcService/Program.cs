using LicensePlateGrpcService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using PaddleOCRSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Online;
using Sdcb.PaddleOCR.Models;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.Models;

namespace LicensePlateGrpcService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Khởi tạo các model/dll OCR
            Yolo yoloCar = new Yolo(new YoloOptions
            {
                OnnxModel = @"C:\TVS\Config\yolov11s.onnx",
                ModelType = ModelType.ObjectDetection,  // Set your model type
                Cuda = false,                           // Use CPU or CUDA for GPU accelerated inference. Default = true
                GpuId = 0,                              // Select Gpu by id. Default = 0
                PrimeGpu = false,                       // Pre-allocate GPU before first inference. Default = false
            });
            Yolo yoloPlate = new Yolo(new YoloOptions
            {
                OnnxModel = @"C:\TVS\Config\license_plate_detector.onnx",      // Your Yolo model in onnx format
                ModelType = ModelType.ObjectDetection,  // Set your model type
                Cuda = false,                           // Use CPU or CUDA for GPU accelerated inference. Default = true
                GpuId = 0,                              // Select Gpu by id. Default = 0
                PrimeGpu = false,                       // Pre-allocate GPU before first inference. Default = false
            });
            builder.Services.AddSingleton(yoloCar);
            builder.Services.AddSingleton(yoloPlate);

            // ==== 🧠 1️⃣ Tải model OCR ====
            FullOcrModel model2 = OnlineFullModels.EnglishV3Slim.DownloadAsync().GetAwaiter().GetResult();

            // ==== 🧠 2️⃣ Đăng ký QueuedPaddleOcrAll ====
            builder.Services.AddSingleton(s =>
            {
                string? paddleDevice = builder.Configuration["PaddleDevice"];
                Action<PaddleConfig> device = paddleDevice == "GPU"
                    ? PaddleDevice.Gpu()
                    : PaddleDevice.Mkldnn();

                return new QueuedPaddleOcrAll(() => new PaddleOcrAll(model2, device)
                {
                    Enable180Classification = false,
                    AllowRotateDetection = false,
                }, consumerCount: 1);
            });

            // ==== 🧠 3️⃣ Đăng ký PaddleOCREngine ====
            builder.Services.AddSingleton(s =>
            {
                string exeDir = AppContext.BaseDirectory;
                var config = new OCRModelConfig
                {
                    det_infer = Path.Combine(exeDir, "inference", "ch_PP-OCRv4_det_infer"),
                    rec_infer = Path.Combine(exeDir, "inference", "ch_PP-OCRv4_rec_infer"),
                    cls_infer = Path.Combine(exeDir, "inference", "ch_ppocr_mobile_v2.0_cls_infer"),
                    keys = Path.Combine(exeDir, "inference", "ppocr_keys.txt")
                };

                var param = new OCRParameter
                {
                    use_angle_cls = false,
                    enable_mkldnn = true,
                    cpu_math_library_num_threads = 2,
                    det_db_score_mode = true,
                    cls = false,
                    det = true
                };

                // dùng Lazy để chỉ khởi tạo khi cần
                return new Lazy<PaddleOCREngine>(() => new PaddleOCREngine(config, param)).Value;
            });

           


            // Add services to the container.
            builder.Services.AddGrpc();

            //var app = builder.Build();

            // Configure the HTTP request pipeline.
            //app.MapGrpcService<GreeterService>();
            //app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            //app.Run();

            //new
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenNamedPipe("OcrPipe", listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            });

            var app = builder.Build();
            app.MapGrpcService<LPServiceImpl>();
            app.MapGet("/", () => "OCR gRPC NamedPipe Server running...");

            app.Run();
        }
    }
}