using Sdcb.PaddleOCR.Models.Online;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using PaddleOCRSharp;

namespace LicensePlateAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // FullOcrModel model =  OnlineFullModels.EnglishV4.DownloadAsync().GetAwaiter().GetResult();
            FullOcrModel model2 = OnlineFullModels.EnglishV3Slim.DownloadAsync().GetAwaiter().GetResult();
           
            // Thêm CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            // Add services to the container.

            builder.Services.AddControllers();


            builder.Services.AddSingleton(s =>
            {
                var test = builder.Configuration["PaddleDevice"];
                Action<PaddleConfig> device = builder.Configuration["PaddleDevice"] == "GPU" ? PaddleDevice.Gpu() : PaddleDevice.Mkldnn();
                //return new QueuedPaddleOcrAll(() => new PaddleOcrAll(LocalFullModels.EnglishV4, device)
                //{
                //    Enable180Classification = false,
                //    AllowRotateDetection = true,
                //}, consumerCount: 1);
                return new QueuedPaddleOcrAll(() => new PaddleOcrAll(model2, device)
                {
                    Enable180Classification = false,
                    AllowRotateDetection = false,
                }, consumerCount: 1);
            });

            builder.Services.AddSingleton(s =>
            {
                OCRModelConfig config = null;
                OCRParameter oCRParameter = new OCRParameter();
                oCRParameter.cpu_math_library_num_threads = 1;
                oCRParameter.enable_mkldnn = true;
                oCRParameter.cls = false;
                oCRParameter.det = true;
                oCRParameter.use_angle_cls = false;
                oCRParameter.det_db_score_mode = true;

                return new PaddleOCREngine(config, oCRParameter);
            });



            // 👉 Thêm Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 👉 Kích hoạt Swagger khi đang ở môi trường Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "License Plate API v1");
                    c.RoutePrefix = string.Empty; // mở Swagger tại root (http://localhost:5000)
                });
            }
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
