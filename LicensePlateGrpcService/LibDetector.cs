using Google.Protobuf;
using LicensePlateGrpcService.Models;
using LicensePlateGrpcService.Utils;
using OpenCvSharp;
using PaddleOCRSharp;
using Sdcb.PaddleOCR;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Tesseract;
using YoloDotNet;
using YoloDotNet.Extensions;
using YoloDotNet.Models; 

namespace LicensePlateGrpcService
{
    public  class LibDetector
    {
        public static bool isSaveImage = false;
        public static string outputFolder = Path.Combine("E:\\MyProject\\yolo\\LicensePlateVN\\LicensePlateVN\\Images", "YoloDotNet_Results");
       
       
        public static async Task<string> DetectByByteImageWrap(byte[] fileBytes, Yolo _yoloCar, Yolo _yoloPlate, QueuedPaddleOcrAll _ocr, PaddleOCREngine _ocrPaddle, RectagInfo rectagInfo)
        {
            
            var licensePlateResult = "";
            try
            {
                fileBytes = ToGrayScale(fileBytes);
                licensePlateResult = await DetectByByteImage(fileBytes, _yoloCar, _yoloPlate, _ocr, _ocrPaddle, rectagInfo);
                if(licensePlateResult.Length < 6)
                {

                    var imageInput_1 = SkiaSharp.SKImage.FromEncodedData(fileBytes);
                    if (rectagInfo.x > 0 && rectagInfo.y > 0 && rectagInfo.width > 0 && rectagInfo.height > 0)
                    {
                        SKRectI rec = new SKRectI(rectagInfo.x, rectagInfo.y, rectagInfo.width + rectagInfo.x, rectagInfo.height + rectagInfo.y);
                        rec = ClampRectToImage(rec, imageInput_1.Width, imageInput_1.Height);
                        imageInput_1 = imageInput_1.Subset(rec);
                        if (isSaveImage == true) imageInput_1.Save(Path.Combine(outputFolder, $"cropt_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.jpeg"), SKEncodedImageFormat.Jpeg, 100);

                    }
                    //  imageInput_1 = ConvertToGray(imageInput_1);
                    List<ObjectDetection> results = _yoloPlate.RunObjectDetection(imageInput_1, 0.1);
                    if (results.Count == 0)
                    {
                        byte[] sampleImageData = ConvertSKImageToByteArray(imageInput_1, SKEncodedImageFormat.Jpeg, 100);
                      //  var imageDataStream = new MemoryStream(sampleImageData);
                      //  var bitmap = new System.Drawing.Bitmap(imageDataStream);
                       // var sKImage = Util.ApplyAdaptiveThreshold(bitmap);
                        var imageInput = SkiaSharp.SKImage.FromEncodedData(sampleImageData);
                        imageInput.Save(Path.Combine(outputFolder, $"ocr_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.jpeg"), SKEncodedImageFormat.Jpeg, 100);
                        results = _yoloPlate.RunObjectDetection(imageInput, 0.005);
                    }
                    if(results.Count == 0)
                    {
                        try
                        {
                           
                            byte[] sampleImageData = ConvertSKImageToByteArray(imageInput_1, SKEncodedImageFormat.Jpeg, 100);

                           
                            licensePlateResult = await OCRImage(sampleImageData, _ocr, _ocrPaddle);
                            if (!String.IsNullOrEmpty(licensePlateResult))
                            {
                                return licensePlateResult;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }

                    foreach (var resultPlate in results)
                    {
                        try
                        {
                            var resultOnePlate = resultPlate;

                            SKRectI recPlate = new SKRectI((int)resultOnePlate.BoundingBox.Left, (int)resultOnePlate.BoundingBox.Top, (int)resultOnePlate.BoundingBox.Right, (int)resultOnePlate.BoundingBox.Bottom);
                            recPlate = ClampRectToImage(recPlate, imageInput_1.Width, imageInput_1.Height);
                            using var imagePlate = imageInput_1.Subset(recPlate);
                            if (isSaveImage == true) imagePlate.Save(Path.Combine(outputFolder, $"snapbak_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.jpeg"), SKEncodedImageFormat.Jpeg, 100);

                            byte[] sampleImageData = ConvertSKImageToByteArray(imagePlate, SKEncodedImageFormat.Jpeg, 100);

                            //using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
                            //{
                            //    licensePlateResult = (await _ocr.Run(src)).Text;
                            //}
                            licensePlateResult = await OCRImage(sampleImageData, _ocr, _ocrPaddle);
                            if (!String.IsNullOrEmpty(licensePlateResult))
                            {
                                return licensePlateResult;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return licensePlateResult;
        }
        public static async Task<string> DetectByByteImage(byte[] fileBytes, Yolo _yoloCar, Yolo _yoloPlate, QueuedPaddleOcrAll _ocr, PaddleOCREngine _ocrPaddle, RectagInfo rectagInfo)
        {
            string outputFolder = Path.Combine("E:\\MyProject\\yolo\\LicensePlateVN\\LicensePlateVN\\Images", "YoloDotNet_Results");
          
            var licensePlateResult = "";
            try
            {
                var imageInput_1 = SkiaSharp.SKImage.FromEncodedData(fileBytes);
                if(rectagInfo.x > 0 && rectagInfo.y> 0 && rectagInfo.width > 0 && rectagInfo.height> 0)
                {
                    SKRectI rec = new SKRectI( rectagInfo.x,  rectagInfo.y,  rectagInfo.width + rectagInfo.x,  rectagInfo.height + rectagInfo.y);
                    rec = ClampRectToImage(rec, imageInput_1.Width, imageInput_1.Height);
                    imageInput_1 = imageInput_1.Subset(rec);
                    if (isSaveImage == true) imageInput_1.Save(Path.Combine(outputFolder, $"cropt_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.jpeg"), SKEncodedImageFormat.Jpeg, 100);

                }

                //test

                if (0 == 0)
                {
                    List<ObjectDetection> results = _yoloPlate.RunObjectDetection(imageInput_1, 0.1);
                    if (results.Count == 0)
                    {
                        results = _yoloPlate.RunObjectDetection(imageInput_1, 0.005);
                    }

                    foreach (var resultPlate in results)
                    {
                        try
                        {
                            var resultOnePlate = resultPlate;

                            SKRectI recPlate = new SKRectI((int)resultOnePlate.BoundingBox.Left, (int)resultOnePlate.BoundingBox.Top, (int)resultOnePlate.BoundingBox.Right, (int)resultOnePlate.BoundingBox.Bottom);
                            recPlate = ClampRectToImage(recPlate, imageInput_1.Width, imageInput_1.Height);
                            using var imagePlate = imageInput_1.Subset(recPlate);
                            byte[] sampleImageData = ConvertSKImageToByteArray(imagePlate, SKEncodedImageFormat.Jpeg, 100);

                            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                            stopwatch.Start();
                            licensePlateResult = await OCRImage(sampleImageData, _ocr, _ocrPaddle);
                            stopwatch.Stop();
                            licensePlateResult += $"_{stopwatch.Elapsed}";
                            if (!String.IsNullOrEmpty(licensePlateResult))
                            {
                                return licensePlateResult;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }

                //endtest


                //thử detect xe 
                List<ObjectDetection> results2 = _yoloCar.RunObjectDetection(imageInput_1, 0.23, 0.7);
                if (results2.Count == 0)
                {
                    results2 = _yoloCar.RunObjectDetection(imageInput_1, 0.1, 0.7);
                }
                if (results2.Count == 0)
                {
                    List<ObjectDetection> results = _yoloPlate.RunObjectDetection(imageInput_1, 0.1);
                    if (results.Count == 0)
                    {
                        results = _yoloPlate.RunObjectDetection(imageInput_1, 0.005);
                    }

                    foreach (var resultPlate in results)
                    {
                        try
                        {
                            var resultOnePlate = resultPlate;

                            SKRectI recPlate = new SKRectI((int)resultOnePlate.BoundingBox.Left, (int)resultOnePlate.BoundingBox.Top, (int)resultOnePlate.BoundingBox.Right, (int)resultOnePlate.BoundingBox.Bottom);
                            recPlate = ClampRectToImage(recPlate, imageInput_1.Width, imageInput_1.Height);
                            using var imagePlate = imageInput_1.Subset(recPlate);
                            byte[] sampleImageData = ConvertSKImageToByteArray(imagePlate, SKEncodedImageFormat.Jpeg, 100);

                            //using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
                            //{
                            //    licensePlateResult = (await _ocr.Run(src)).Text;
                            //}
                            licensePlateResult = await OCRImage(sampleImageData, _ocr, _ocrPaddle);
                            if (!String.IsNullOrEmpty(licensePlateResult))
                            {
                                return licensePlateResult;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
                foreach (var result in results2)
                {
                    // many cars
                    try
                    {
                        var resultOne = result;

                        SKRectI rec = new SKRectI((int)resultOne.BoundingBox.Left, (int)resultOne.BoundingBox.Top, (int)resultOne.BoundingBox.Right, (int)resultOne.BoundingBox.Bottom);
                        rec = ClampRectToImage(rec, imageInput_1.Width, imageInput_1.Height);
                        using var image = imageInput_1.Subset(rec);
                        //dùng để save image cắt biển số
                        if(isSaveImage == true) image.Save(Path.Combine(outputFolder, $"snap_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.jpeg"), SKEncodedImageFormat.Jpeg, 100);
                         
                        List<ObjectDetection> results = _yoloPlate.RunObjectDetection(image, 0.1);
                        if (results.Count == 0)
                        {
                            results = _yoloPlate.RunObjectDetection(image, 0.05);
                        }
                        if(results2.Count == 1 && results.Count == 0)
                        {
                            // boc lot
                            byte[] sampleImageData = ConvertSKImageToByteArray(imageInput_1, SKEncodedImageFormat.Jpeg, 100);
                            //using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
                            //{
                            //    licensePlateResult = (await _ocr.Run(src)).Text;
                            //}
                            licensePlateResult = await OCRImage(sampleImageData, _ocr, _ocrPaddle);
                            if (!String.IsNullOrEmpty(licensePlateResult))
                            {
                                
                                return licensePlateResult;
                            }
                        }
                        foreach (var resultPlate in results)
                        {
                            try
                            {
                                var resultOnePlate = resultPlate;

                                SKRectI recPlate = new SKRectI((int)resultOnePlate.BoundingBox.Left, (int)resultOnePlate.BoundingBox.Top, (int)resultOnePlate.BoundingBox.Right, (int)resultOnePlate.BoundingBox.Bottom);
                                recPlate = ClampRectToImage(recPlate, image.Width, image.Height);
                                //save to view
                                using var imagePlate = image.Subset(recPlate);
                                //dùng để save image cắt biển số 
                                if (isSaveImage == true)  imagePlate.Save(Path.Combine(outputFolder, $"snap_{DateTime.Now.Second}.jpeg"), SKEncodedImageFormat.Jpeg, 80);
                                byte[] sampleImageData = ConvertSKImageToByteArray(imagePlate, SKEncodedImageFormat.Jpeg, 100);

                                licensePlateResult = await OCRImage(sampleImageData, _ocr, _ocrPaddle);
                                if (!String.IsNullOrEmpty(licensePlateResult))
                                {
                                    return licensePlateResult;
                                }
                            }
                            catch (Exception e)
                            {
                                byte[] sampleImageData = ConvertSKImageToByteArray(image, SKEncodedImageFormat.Jpeg, 100);
                                licensePlateResult = await OCRImage(sampleImageData, _ocr, _ocrPaddle);
                                if (!String.IsNullOrEmpty(licensePlateResult))
                                {
                                    return licensePlateResult;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
                // khi nhan dien xe ma khong nhan dien duoc bien so
                if(results2.Count > 0)
                {
                    foreach (var result in results2)
                    {
                        // many cars
                        try
                        {
                            var resultOne = result;

                            SKRectI rec = new SKRectI((int)resultOne.BoundingBox.Left, (int)resultOne.BoundingBox.Top, (int)resultOne.BoundingBox.Right, (int)resultOne.BoundingBox.Bottom);
                            rec = ClampRectToImage(rec, imageInput_1.Width, imageInput_1.Height);
                            using var image = imageInput_1.Subset(rec);
                            //dùng để save image cắt biển số
                            if (isSaveImage == true) image.Save(Path.Combine(outputFolder, $"snap_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.jpeg"), SKEncodedImageFormat.Jpeg, 100);


                            // boc lot
                            byte[] sampleImageData = ConvertSKImageToByteArray(image, SKEncodedImageFormat.Jpeg, 100);
                            
                            licensePlateResult = await OCRImage(sampleImageData, _ocr, _ocrPaddle);
                            if (!String.IsNullOrEmpty(licensePlateResult))
                            {

                                return licensePlateResult;
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return licensePlateResult;
        }
        public static async Task<string> OCRImage(byte[] fileBytes, QueuedPaddleOcrAll _ocr, PaddleOCREngine _ocrPaddle)
        {
            // var licensePlateResult = await OCRImage_Old(fileBytes, _ocr);
            //var licensePlateResult = await OCRImage_Old(fileBytes, _ocr);
            var licensePlateResult = await OCRImagePad44(fileBytes, _ocrPaddle);

            return licensePlateResult;
        }
        public static async Task<string> OCRImage_Old(byte[] fileBytes, QueuedPaddleOcrAll _ocr)
        {
            var licensePlateResult = "";
            var temp = "";
            try
            {
                using (Mat src = Cv2.ImDecode(fileBytes, ImreadModes.Color))
                {
                    licensePlateResult = (await _ocr.Run(src)).Text;
                    temp = licensePlateResult;
                }
            }
            catch (Exception ex)
            {
                 
            }
            licensePlateResult = Util.FormatLicensePlate(licensePlateResult);
            var checkValid = CheckValidLicense(licensePlateResult);
            if (checkValid)
            {
                return licensePlateResult;
            }
            else
            {
                try
                {
                    using (Mat src = Cv2.ImDecode(fileBytes, ImreadModes.Grayscale))
                    {
                        licensePlateResult = (await _ocr.Run(src)).Text;
                        if (licensePlateResult.Length >= 7) {
                            licensePlateResult = Util.FormatLicensePlate(licensePlateResult);
                            checkValid = CheckValidLicense(licensePlateResult);
                            return checkValid ? licensePlateResult : "";
                        }

                    }
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
            return temp;
        }
        public static async Task<string> OCRImagePad44(byte[] fileBytes, PaddleOCREngine _ocrPaddle)
        {
            var licensePlateResult = "";
            var temp = "";
            try
            {
                var ocrResult = _ocrPaddle.DetectText(fileBytes); 
                if(ocrResult.TextBlocks.Count == 2)
                {
                    var line1 = ocrResult.TextBlocks[0].Text;
                    var line2 = ocrResult.TextBlocks[1].Text;
                    if(line1.Length == 3)
                    {
                        licensePlateResult = line1+ line2;
                    }
                    else
                    {
                        // licensePlateResult = line2 + line1;
                        licensePlateResult = line1 + line2;
                    }
                }
                else
                {
                    foreach (var itemText in ocrResult.TextBlocks)
                    {
                        licensePlateResult += itemText.Text;
                    }
                }
               
                temp = licensePlateResult;
            }
            catch (Exception ex)
            {

            }
            licensePlateResult = Util.FormatLicensePlate(licensePlateResult);
            var checkValid = CheckValidLicense(licensePlateResult);
            if (checkValid)
            {
                return licensePlateResult;
            }
            else
            {
                try
                {
                    using (Mat src = Cv2.ImDecode(fileBytes, ImreadModes.Grayscale))
                    {
                        var ocrResult = _ocrPaddle.DetectText(src.ToBytes());
                        foreach (var itemText in ocrResult.TextBlocks)
                        {
                            licensePlateResult += itemText;
                        }
                        temp = licensePlateResult;
                        if (licensePlateResult.Length >= 7)
                        {
                            licensePlateResult = Util.FormatLicensePlate(licensePlateResult);
                            checkValid = CheckValidLicense(licensePlateResult);
                            return checkValid ? licensePlateResult : "";
                        }

                    }
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
            return temp;
        }

        public static byte[] ToGrayScale(byte[] fileBytes)
        {
            // Chuyển byte[] thành Mat (ảnh gốc)
            using var ms = new MemoryStream(fileBytes);
            var buffer = ms.ToArray();
            using var src = Cv2.ImDecode(buffer, ImreadModes.Color);

            if (src.Empty())
                throw new Exception("Không đọc được ảnh từ byte[]");

            // Chuyển sang grayscale
            using var gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            // Encode lại sang JPG hoặc PNG (tùy bạn)
            return gray.ImEncode(".jpg");
        }
        public static async Task<string> OCRTesseract(byte[] fileBytes, QueuedPaddleOcrAll _ocr)
        {
            var licensePlateResult = "";
            var temp = "";
            try
            {
                var engine = new TesseractEngine(@"C:\TVS\DataSet", "eng", EngineMode.Default);
                var image = Pix.LoadFromMemory(fileBytes);
                var page = engine.Process(image);

                var text = page.GetText();

            }
            catch (Exception ex)
            {

            }
            try
            {
                //var imageDataStream = new MemoryStream(fileBytes);
                //var bitmap = new System.Drawing.Bitmap(imageDataStream);
                //var sKImage = Util.ApplyAdaptiveThreshold(bitmap);
                //var imageInput = SkiaSharp.SKImage.FromEncodedData(sKImage);
                //imageInput.Save(Path.Combine(outputFolder, $"ocr_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.jpeg"), SKEncodedImageFormat.Jpeg, 100);
                //byte[] sampleImageData = ConvertSKImageToByteArray(imageInput, SKEncodedImageFormat.Jpeg, 80);
                using (Mat src = Cv2.ImDecode(fileBytes, ImreadModes.Color))
                {

                    licensePlateResult = (await _ocr.Run(src)).Text;
                    temp = licensePlateResult;
                }
            }
            catch (Exception ex)
            {

            }
            licensePlateResult = Util.FormatLicensePlate(licensePlateResult);
            var checkValid = CheckValidLicense(licensePlateResult);
            if (checkValid)
            {
                return licensePlateResult;
            }
            else
            {
                try
                {
                    using (Mat src = Cv2.ImDecode(fileBytes, ImreadModes.Grayscale))
                    {
                        licensePlateResult = (await _ocr.Run(src)).Text;
                        if (licensePlateResult.Length >= 7)
                        {
                            licensePlateResult = Util.FormatLicensePlate(licensePlateResult);
                            checkValid = CheckValidLicense(licensePlateResult);
                            return checkValid ? licensePlateResult : "";
                        }

                    }
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
            return temp;
        }
        public static bool CheckValidLicense(string plateNumber)
        {
            var check = true;
            try
            {
                var preArr = plateNumber.ToArray();
                if (!Char.IsDigit(preArr[0])) return false;
                if (!Char.IsDigit(preArr[1])) return false;
                if (!Char.IsLetter(preArr[2])) return false;
            }
            catch (Exception ex)
            {
                check = false;
            }
            return check;
        }
        public static SKImage AdaptiveThreshold(SKImage original)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original), "SKImage cannot be null.");
            }
            // Create a new bitmap for the grayscale image
            using (var originalBitmap = SKBitmap.FromImage(original))
            {
                // Create a new bitmap for the thresholded image
                using (var thresholdedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height))
                {
                    int windowSize = 15; // Size of the local region
                    int halfWindow = windowSize / 2;

                    // Create a 2D array to store pixel intensity
                    float[,] pixelIntensities = new float[originalBitmap.Width, originalBitmap.Height];

                    // Fill pixel intensity array
                    for (int y = 0; y < originalBitmap.Height; y++)
                    {
                        for (int x = 0; x < originalBitmap.Width; x++)
                        {
                            var color = originalBitmap.GetPixel(x, y);
                            pixelIntensities[x, y] = (color.Red + color.Green + color.Blue) / 3.0f;
                        }
                    }

                    // Apply adaptive thresholding
                    for (int y = 0; y < originalBitmap.Height; y++)
                    {
                        for (int x = 0; x < originalBitmap.Width; x++)
                        {
                            // Calculate local average intensity
                            float localSum = 0;
                            int count = 0;

                            for (int dy = -halfWindow; dy <= halfWindow; dy++)
                            {
                                for (int dx = -halfWindow; dx <= halfWindow; dx++)
                                {
                                    int neighborY = y + dy;
                                    int neighborX = x + dx;

                                    // Check bounds
                                    if (neighborY >= 0 && neighborY < originalBitmap.Height &&
                                        neighborX >= 0 && neighborX < originalBitmap.Width)
                                    {
                                        localSum += pixelIntensities[neighborX, neighborY];
                                        count++;
                                    }
                                }
                            }

                            // Calculate local average
                            float localAverage = localSum / count;
                            byte thresholdedValue = pixelIntensities[x, y] > localAverage ? (byte)255 : (byte)0;

                            // Set the thresholded pixel
                            thresholdedBitmap.SetPixel(x, y, new SKColor(thresholdedValue, thresholdedValue, thresholdedValue, 255));
                        }
                    }

                    // Save the thresholded image
                    using (var thresholdedImage = SKImage.FromBitmap(thresholdedBitmap))
                        return thresholdedImage; ;
                }
            }
        }
        public static SKImage ConvertToGray(SKImage original)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original), "SKImage cannot be null.");
            }
            // Create a new bitmap for the grayscale image
            using (var originalBitmap = SKBitmap.FromImage(original))
            {
                // Create a new bitmap for the grayscale image
                using (var grayscaleBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height))
                {
                    // Prepare the grayscale pixel data
                    var grayscalePixels = new byte[grayscaleBitmap.ByteCount];

                    float contrastFactor = 1.5f; // Increase contrast (1.0 = no change, >1.0 = increase)


                    for (int y = 0; y < originalBitmap.Height; y++)
                    {
                        for (int x = 0; x < originalBitmap.Width; x++)
                        {
                            // Get the original pixel color
                            var color = originalBitmap.GetPixel(x, y);

                            // Calculate the grayscale value
                            byte grayValue = (byte)(0.3 * color.Red + 0.59 * color.Green + 0.11 * color.Blue);

                            // Set the grayscale pixel
                            int pixelIndex = (y * 4 * originalBitmap.Width) + (x * 4);
                            //grayscalePixels[pixelIndex] = grayValue;     // Blue
                            //grayscalePixels[pixelIndex + 1] = grayValue; // Green
                            //grayscalePixels[pixelIndex + 2] = grayValue; // Red
                            //grayscalePixels[pixelIndex + 3] = color.Alpha; // Alpha



                            grayscalePixels[pixelIndex] = Clamp((byte)(contrastFactor * (grayValue - 128) + 128));     // Blue
                            grayscalePixels[pixelIndex + 1] = Clamp((byte)(contrastFactor * (grayValue - 128) + 128)); // Green
                            grayscalePixels[pixelIndex + 2] = Clamp((byte)(contrastFactor * (grayValue - 128) + 128)); // Red
                            grayscalePixels[pixelIndex + 3] = color.Alpha; // Alpha


                            //byte r = Clamp((byte)(contrastFactor * (color.Red - 128) + 128));
                            //byte g = Clamp((byte)(contrastFactor * (color.Green - 128) + 128));
                            //byte b = Clamp((byte)(contrastFactor * (color.Blue - 128) + 128));

                            //grayscalePixels[pixelIndex] = b;   // Blue
                            //grayscalePixels[pixelIndex + 1] = g; // Green
                            //grayscalePixels[pixelIndex + 2] = r; // Red
                            //grayscalePixels[pixelIndex + 3] = color.Alpha; // Alpha
                        }
                    }

                    // Set the grayscale pixels to the bitmap
                    // Pin the byte array and set the grayscale pixels
                    GCHandle handle = GCHandle.Alloc(grayscalePixels, GCHandleType.Pinned);
                    try
                    {
                        nint pixelPtr = handle.AddrOfPinnedObject();
                        grayscaleBitmap.SetPixels(pixelPtr);
                    }
                    finally
                    {
                        handle.Free();
                    }

                    // Create an SKImage from the grayscale bitmap
                    using (var grayscaleImage = SKImage.FromBitmap(grayscaleBitmap))

                        return SKImage.FromBitmap(grayscaleBitmap);
                }
            }
        }
        static byte Clamp(int value)
        {
            return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
        }
        public static byte[] ConvertSKImageToByteArray(SKImage skImage, SKEncodedImageFormat format = SKEncodedImageFormat.Png, int quality = 100)
        {
            if (skImage == null)
            {
                throw new ArgumentNullException(nameof(skImage), "SKImage cannot be null.");
            }
            using (var imageData = skImage.Encode(format, quality))
            {
                return imageData.ToArray();
            }
        }
        static SKRectI ClampRectToImage(SKRectI rect, int imageWidth, int imageHeight)
        {
            // Giới hạn giá trị bên trái và phải
            int left = Math.Max(0, rect.Left);
            int top = Math.Max(0, rect.Top);
            int right = Math.Min(imageWidth, rect.Right);
            int bottom = Math.Min(imageHeight, rect.Bottom);

            // Nếu hình chữ nhật có chiều rộng và chiều cao hợp lệ
            if (right > left && bottom > top)
            {
                return new SKRectI(left, top, right, bottom);
            }
            else
            {
                // Nếu không có diện tích hợp lệ, trả về một hình chữ nhật rỗng
                return new SKRectI(0, 0, 0, 0);
            }
        }
    }
}
