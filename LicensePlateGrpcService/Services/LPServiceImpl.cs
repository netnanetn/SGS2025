using Grpc.Core;
using LicensePlateGrpcService.Models;
using PaddleOCRSharp;
using Sdcb.PaddleOCR;
using System.Diagnostics;
using YoloDotNet;

namespace LicensePlateGrpcService.Services
{
    public class LPServiceImpl : OcrService.OcrServiceBase
    {
        private readonly Yolo _yoloCar;
        private readonly Yolo _yoloPlate;
        private readonly QueuedPaddleOcrAll _ocr;
        private readonly PaddleOCREngine _ocrPaddle;
        public LPServiceImpl(Yolo yoloCar, Yolo yoloPlate, QueuedPaddleOcrAll ocr, PaddleOCREngine ocrPaddle)
        {
            _yoloCar = yoloCar;
            _yoloPlate = yoloPlate;
            _ocr = ocr;
            _ocrPaddle = ocrPaddle;
        }
        public override async Task<OcrReply> Recognize(OcrRequest request, ServerCallContext context)
        {
            // Giả lập OCR xử lý
            //return Task.FromResult(new OcrReply
            //{
            //    Text = $"Biển số nhận diện: {request.CompanyCode}",
            //    TimeMs = 180
            //});
            var sw = Stopwatch.StartNew();
            string result = "";

            try
            {
                // Giải mã ảnh từ base64
                var imageBytes = Convert.FromBase64String(request.Base64);

                // Tạo RectagInfo
                var rect = new RectagInfo
                {
                    x = request.X,
                    y = request.Y,
                    width = request.Width,
                    height = request.Height
                };

                // Gọi vào LibDetector
                result = await LibDetector.DetectByByteImageWrap(
                    imageBytes, _yoloCar, _yoloPlate, _ocr, _ocrPaddle, rect
                );
            }
            catch (Exception ex)
            {
                result = $"❌ Lỗi OCR: {ex.Message}";
            }

            sw.Stop();
            return new OcrReply
            {
                Text = result ?? "",
                TimeMs = (int)sw.ElapsedMilliseconds
            };
        }
    }
}
