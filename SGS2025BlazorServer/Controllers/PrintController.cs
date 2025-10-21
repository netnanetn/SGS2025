using CMS_Data.ModelDTO;
using CMS_Data.Models;
using CMS_Data.Services;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Mvc;
using Razor.Templating.Core;

namespace SGS2025BlazorServer.Controllers
{
    public class PrintController : Controller
    {
        private ScaleService _scaleService;
        private CompanyService _companyService;
        public PrintController(ScaleService scaleService, CompanyService companyService)
        {
            _scaleService = scaleService;
            _companyService = companyService;
        }
        // Route: /print/test
        [HttpGet("/print/test/{id:int}")]
        public async Task<IActionResult> Test(int id)
        {
            // Bạn có thể gán model mẫu để test
            var company = await _companyService.GetByIdAsync();
            var scale = await _scaleService.GetById(id);
            if(scale == null)
                return Content($"❌ Không tìm thấy dữ liệu phiếu cân có Id = {id}");
            scale.Img11 = "https://placehold.co/200x150?text=%E1%BA%A2nh+1";
            scale.Img12 = "https://placehold.co/200x150?text=%E1%BA%A2nh+1";
            scale.Img13 = "https://placehold.co/200x150?text=%E1%BA%A2nh+1";
            scale.Img21 = "https://placehold.co/200x150?text=%E1%BA%A2nh+1";
            scale.Img22 = "https://placehold.co/200x150?text=%E1%BA%A2nh+1";
            scale.Img23 = "https://placehold.co/200x150?text=%E1%BA%A2nh+1";

            var modelDTO = new PrintScaleDTO
            {
                Scale = scale,
                Company = company
            };
            // View path mặc định: Views/Print/html6anh.cshtml
            return View("html_a5_ngang", modelDTO);
        }
    }
}
