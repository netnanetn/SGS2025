using System.ComponentModel.DataAnnotations;

namespace LicensePlateAPI.Models
{
    public class UploadImageDTO : RectagInfo
    {
        public string companyCode { get; set; }
        public string base64 { get; set; }
    }
    public class RectagInfo
    {
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int width { get; set; } = 0;
        public int height { get; set; } = 0;
    }
    public class FileUploadRequest : RectagInfo
    {
        [Required]
        public IFormFile filea { get; set; }

    }
}
