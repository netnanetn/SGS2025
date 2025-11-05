using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System;
using YoloDotNet;
using YoloDotNet.Models;
using YoloDotNet.Enums;
using Sdcb.PaddleOCR;
using PaddleOCRSharp;
using YoloDotNet.Core;
using SkiaSharp;
using LicensePlateAPI.Models;

namespace LicensePlateAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : ControllerBase, IDisposable
    {
        private const int PlusPixel = 50;

        private readonly ILogger<LicenseController> _logger;

        private static readonly Yolo _yolo = new Yolo(new YoloOptions
        {
            OnnxModel = Path.Combine("ModelYolo", "license_plate_detector.onnx"),
            ExecutionProvider = new CpuExecutionProvider(), // hoặc CudaExecutionProvider nếu có GPU
            ImageResize = ImageResize.Proportional,        // Resize ảnh đầu vào
            SamplingOptions = new SKSamplingOptions(SKFilterMode.Linear)
        });

        private static readonly Yolo _yolo11 = new Yolo(new YoloOptions
        {
            OnnxModel = Path.Combine("ModelYolo", "yolov11s.onnx"),
            ExecutionProvider = new CpuExecutionProvider(),
            ImageResize = ImageResize.Proportional,
            SamplingOptions = new SKSamplingOptions(SKFilterMode.Linear)
        });

        private readonly QueuedPaddleOcrAll _ocr;
        private readonly PaddleOCREngine _ocrPaddle;
        public LicenseController(ILogger<LicenseController> logger, QueuedPaddleOcrAll ocr, PaddleOCREngine ocrPaddle)
        {
            _logger = logger;
            _ocr = ocr;
            _ocrPaddle = ocrPaddle;
        }

        [HttpPost]
        [Route("detect-base64")]
        public async Task<ActionResult<Object>> DetectByBase64(UploadImageDTO modelDTO)
        {
            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                var resultLicense = "";
                if (string.IsNullOrEmpty(modelDTO.companyCode) || string.IsNullOrEmpty(modelDTO.base64)) return new
                {
                    StatusCode = 500,
                    Message = "Thiếu thông tin"
                };
                stopwatch.Start();
                var imageDataByteArray = Convert.FromBase64String(modelDTO.base64);

                var rectagInfo = new RectagInfo
                {
                    x = modelDTO.x,
                    y = modelDTO.y,
                    width = modelDTO.width,
                    height = modelDTO.height,
                };
                resultLicense = await LibDetector.DetectByByteImageWrap(imageDataByteArray, _yolo11, _yolo, _ocr, _ocrPaddle, rectagInfo);

                stopwatch.Stop();
                if (!String.IsNullOrEmpty(resultLicense))
                {
                    return new
                    {
                        StatusCode = 200,
                        Message = "Nhận diện thành công",
                        TimeWatch = stopwatch.Elapsed,
                        LicensePlate = resultLicense
                    };
                }
                return new
                {
                    StatusCode = 404,
                    Message = "NotFound",
                    TimeWatch = stopwatch.Elapsed,
                    LicensePlate = resultLicense
                };


            }
            catch (Exception ex)
            {

            }
            return new
            {
                StatusCode = 500,
                Message = "Có lỗi trong quá trình upload ảnh"
            };
        }
        [HttpPost]
        [Route("detect-file")]
        public async Task<ActionResult<Object>> DetectByFile(FileUploadRequest model)
        {
            try
            {

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                string outputFolder = Path.Combine("E:\\MyProject\\yolo\\LicensePlateVN\\LicensePlateVN\\Images", "YoloDotNet_Results");
                var resultLicense = "";
                var formCollection = await Request.ReadFormAsync();
                var file = formCollection.Files.First();
                stopwatch.Start();
                using (var msImage = new MemoryStream())
                {
                    await file.CopyToAsync(msImage);
                    var fileBytes = msImage.ToArray();
                    stopwatch.Start();

                    //var rectagInfo = new RectagInfo
                    //{
                    //    x = model.x,
                    //    y = model.y,
                    //    width = model.width,
                    //    height = model.height,
                    //};
                    var rectagInfo = new RectagInfo
                    {
                        x = 606,
                        y = 0,
                        width = 1242,
                        height = 756,
                    };
                    resultLicense = await LibDetector.DetectByByteImageWrap(fileBytes, _yolo11, _yolo, _ocr, _ocrPaddle, rectagInfo);

                    stopwatch.Stop();
                    if (!String.IsNullOrEmpty(resultLicense))
                    {
                        return new
                        {
                            StatusCode = 200,
                            Message = "Nhận diện thành công",
                            TimeWatch = stopwatch.Elapsed,
                            LicensePlate = resultLicense
                        };
                    }
                    return new
                    {
                        StatusCode = 404,
                        Message = "NotFound",
                        TimeWatch = stopwatch.Elapsed,
                        LicensePlate = resultLicense
                    };
                }
            }
            catch (Exception ex)
            {

            }
            return new
            {
                StatusCode = 500,
                Message = "Error"
            };
        }
        [HttpPost]
        [Route("GeLicensePlateByUrl")]
        public async Task<ActionResult<Object>> GeLicensePlateByUrl(ImageRequestUrlModel model)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            var resultLicense = "";
            try
            {
                var base64 = ConvertImageURLToBase64(model.UrlImage);

                var imageDataByteArray = Convert.FromBase64String(base64);
                stopwatch.Start();

                var rectagInfo = new RectagInfo
                {
                    x = model.x,
                    y = model.y,
                    width = model.width,
                    height = model.height,
                };
                resultLicense = await LibDetector.DetectByByteImageWrap(imageDataByteArray, _yolo, _yolo, _ocr, _ocrPaddle, rectagInfo);
                stopwatch.Stop();
                if (!String.IsNullOrEmpty(resultLicense))
                {
                    return new
                    {
                        StatusCode = 200,
                        Message = "Nhận diện thành công",
                        TimeWatch = stopwatch.Elapsed,
                        LicensePlate = resultLicense
                    };
                }
                return new
                {
                    StatusCode = 404,
                    Message = "NotFound",
                    TimeWatch = stopwatch.Elapsed,
                    LicensePlate = resultLicense
                };
            }
            catch (Exception ex)
            {

            }
            return new
            {
                StatusCode = 404,
                Message = "Không tìm thấy biển số xe",
                IdImage = model.IdImage,
                LicensePlate = ""
            };
        }


   

        public static String ConvertImageURLToBase64(String url)
        {
            StringBuilder _sb = new StringBuilder();
            Byte[] _byte = GetImage(url);
            _sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));

            return _sb.ToString();
        }

      
     
        private static byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;

            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                buf = null;
            }

            return (buf);
        }

        public void Dispose()
        {
            //_yolo.Dispose();
            //_yolo11.Dispose();
        }


    }
    public class ImageRequestModel
    {
        public string keyCode { get; set; }
        public string ImageBase64 { get; set; }
    }
    public class ImageRequestUrlModel : RectagInfo
    {
        public int? IdScale { get; set; }
        public int? IdImage { get; set; }
        public string UrlImage { get; set; }
        public string Code { get; set; }
    }
    public class ResultItemDTO
    {
        public string UrlImage { get; set; }
        public TimeSpan TimeProcess { get; set; }
        public string LicensePlate { get; set; }
    }
}
