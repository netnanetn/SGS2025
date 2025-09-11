using RazorLight;

namespace SGS2025Client.Components.HelperService
{
    public class RazorRenderer
    {
        private readonly RazorLightEngine _engine;
        private readonly string _templatesDir;

        public RazorRenderer()
        {
            _templatesDir = Path.Combine(AppContext.BaseDirectory, "Components", "PdfTemplates"); 
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(_templatesDir)   // 🔥 load template từ thư mục build
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
        {
            var templatePath = Path.Combine(_templatesDir, templateName);
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Không tìm thấy template: {templatePath}");
            }

            // Chỉ truyền tên file, không kèm path
            return await _engine.CompileRenderAsync(templateName, model);
           
        }
    }
}
