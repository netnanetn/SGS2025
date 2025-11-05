 
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Drawing;
using SkiaSharp;
using Microsoft.AspNetCore.Components.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace LicensePlateAPI.Utils
{
    public class Util
    {
        public static string FormatLicensePlate(string license)
        {
			var res = "";
			try
			{


                //process line
                var isOneLine = license.IndexOf("\n") > -1 ? false : true;

                if (isOneLine)
                {
                    
                }
                else
                {
                    var licenSplits = license.Split("\n");
                    var tempLicense = "";
                    if(licenSplits.Length == 2)
                    {
                        var f1 = licenSplits[0].Length > 3 ? licenSplits[0].Substring(0,3) : licenSplits[0];
                        tempLicense = f1 + licenSplits[1];
                    }
                    else if(licenSplits.Length == 3)
                    {
                        var line1 = licenSplits[0];
                        if (line1.Length > 2)
                        {
                            var preArrF = line1.ToArray();
                            if(Char.IsDigit(preArrF[0]) && Char.IsDigit(preArrF[1]) && Char.IsLetter(preArrF[2]))
                            {
                                tempLicense += line1.Substring(0,3);
                            }
                        }

                        var line2 = licenSplits[1];
                        if (line2.Length > 2)
                        {
                            if (!string.IsNullOrEmpty(tempLicense))
                            {
                                tempLicense += line2;
                            }
                            else
                            {
                                tempLicense += line2;
                            } 
                        }

                        var line3 = licenSplits[2];
                        if (line3.Length > 2 && tempLicense.Length < 5)
                        {
                            tempLicense += line3;
                        }

                    }
                    if(tempLicense != "") license = tempLicense;

                    //var preLicense = licenSplits[0];
                    //var preArr = preLicense.ToArray();
                    //var key3 = convertCharacter(preLicense[preArr.Length - 1].ToString(), false);
                    //var key2 = convertCharacter(preLicense[preArr.Length - 2].ToString(), true);
                    //var key1 = convertCharacter(preLicense[preArr.Length - 3].ToString(), true);
                    //var lastLicense = licenSplits[1];
                    //var lastLicenseConvert = convertCharacter(lastLicense, true);
                    //res = $"{key1}{key2}{key3}{lastLicenseConvert}";
                }






                license = replaceSpecial(license);
    //            var isOneLine = license.IndexOf("n") > -1 ? false : true;

    //            if (isOneLine)
				//{
				//	var licenSplits = license.Split('-');
    //                var preLicense  = licenSplits[0];
    //                var preArr = preLicense.ToArray();
    //                var key3 = convertCharacter(preLicense[preArr.Length - 1].ToString(), false);
    //                var key2 = convertCharacter(preLicense[preArr.Length - 2].ToString(), true);
    //                var key1 = convertCharacter(preLicense[preArr.Length - 3].ToString(), true);
    //                var lastLicense = licenSplits[1];
    //                var lastLicenseConvert = convertCharacter(lastLicense, true);
    //                res = $"{key1}{key2}{key3}{lastLicenseConvert}";
    //            }
				//else
				//{
    //                var licenSplits = license.Split("n");
    //                var preLicense = licenSplits[0];
    //                var preArr = preLicense.ToArray();
    //                var key3 = convertCharacter(preLicense[preArr.Length - 1].ToString(), false);
    //                var key2 = convertCharacter(preLicense[preArr.Length - 2].ToString(), true);
    //                var key1 = convertCharacter(preLicense[preArr.Length - 3].ToString(), true);
    //                var lastLicense = licenSplits[1];
    //                var lastLicenseConvert = convertCharacter(lastLicense, true);
    //                res = $"{key1}{key2}{key3}{lastLicenseConvert}";
    //            }
                res = replaceSpecial(license);
                var length = 8; //29D078811
                if (res.Length > length)
                {
                    if (res.Contains("A")) return res.Substring(0, length - 0);
                    if (res.Contains("S")) return res.Substring(0, length - 1);
                    if (res.Contains("Y")) return res.Substring(0, length - 1);
                    if (res.Contains("Z")) return res.Substring(0, length - 1);
                }
                var preLicense = res;
                var preArr = preLicense.ToArray();
                var key1 = convertCharacter(preLicense[0].ToString(), true);
                var key2 = convertCharacter(preLicense[1].ToString(), true);
                var key3 = convertCharacter(preLicense[2].ToString(), false);
               
               
                var lastLicenseConvert = preLicense.Substring(3);
                res = $"{key1}{key2}{key3}{lastLicenseConvert}";
                if (res.Length > length) res = res.Substring(0, length);

            }
            catch (Exception ex)
			{
				res = license;
			}
          
			return res;
        }
        public static string replaceSpecial(string license)
        { 
            return Regex.Replace(license.Replace("-",""), "[^0-9A-Za-z _-]", "");
        }
        private static String convertCharacter(string text, bool charToInt)
        {
            if (String.IsNullOrEmpty(text)) return text;
            String[] characters_1 =    { "O", "I", "J", "A", "G", "B", "S", "U" };
            String[] ditCharacters_1 = { "0", "1", "3", "4", "6", "8", "5", "0" };

            String[] characters_2 =    { "0", "1", "3", "4", "6", "5", "8", "U", "I" };
            String[] ditCharacters_2 = { "D", "I", "J", "A", "G", "S", "B", "D", "A" };

            var result = charToInt ? replaceString(text, characters_1, ditCharacters_1) : replaceString(text, characters_2, ditCharacters_2);

            return result;

        }
        static String replaceString(string text, String[] pattern, String[] replace)
        {
            var resultString = "";
            try
            {

                char[] characters = text.ToCharArray();
                for (int i = 0; i < characters.Length; i++)
                {

                    resultString += ReplaceChar(characters[i].ToString(), pattern, replace);
                }
            }
            catch (Exception ex)
            {

            }


            return resultString;
        }
        static string ReplaceChar(string text, String[] pattern, String[] replace)
        {
            var result = "";
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == text)
                {
                    result = replace[i];
                    break;
                }

            }
            if (result == "")
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (pattern[i] == text.ToLower())
                    {
                        result = replace[i];
                        break;
                    }

                }
            }
            if (result == "") result = text;
            return result;
        } 
        public static SKImage ResizeBitmapToSKImage(SKImage image, int cropWidth, int cropHeight)
        {
            var t = 600;
            // Lấy kích thước gốc
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // Tính toán vị trí cắt
            int cropX = (originalWidth - cropWidth + 800) / 2  ;
            int cropY = (originalHeight - cropHeight - 600) / 2  ;

            // Tạo bitmap mới cho ảnh đã cắt
            using var croppedBitmap = new SKBitmap(cropWidth, cropHeight);

            using (var canvas = new SKCanvas(croppedBitmap))
            {
                // Vẽ phần đã cắt từ ảnh gốc lên bitmap mới
                var srcRect = new SKRect(cropX, cropY, cropX + cropWidth, cropY + cropHeight);
                var destRect = new SKRect(0, 0, cropWidth, cropHeight);
                canvas.DrawImage(image, srcRect, destRect);
            }

            // Trả về ảnh đã cắt dưới dạng SKImage
            return SKImage.FromBitmap(croppedBitmap);





            //using var canvasBitmap = new SKBitmap(canvasWidth, canvasHeight);

            //using (var canvas = new SKCanvas(canvasBitmap))
            //{
            //    // Clear the canvas with a white background (optional)
            //    canvas.Clear(SKColors.White);

            //    // Calculate the position to center the image
            //    float x = (canvasWidth - image.Width) / 2f;
            //    float y = (canvasHeight - image.Height) / 2f;

            //    // Draw the original image centered on the canvas
            //    canvas.DrawImage(image, x, y);
            //}

            //// Return the centered image as SKImage
            //return SKImage.FromBitmap(canvasBitmap);




             
        }
    }
}
