using ImageAnalyzer.Helper;
using ImageAnalyzer.Models;
using ImageAnalyzer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageAnalyzer.Controllers
{
    public class ImageController: Controller
    {
        private readonly GeminiService _gemini;

        public ImageController(GeminiService gemini)
        {
            _gemini = gemini;
        }

        public IActionResult Index() => View();

        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
        public async Task<IActionResult> Analyze(IFormFile file, string language = null)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select an image file.");
                return View("Upload");
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var result = new ResponseModel();
            try
            {
                var mimeType = MimeTypeHelper.GetMimeType(file.FileName);
                var content = await _gemini.AnalyzeImageAsync(bytes, mimeType, language);
                result.Alts = content.alts;
                result.Captions = content.captions;
            }
            catch (Exception ex)
            {
                //error handling
                TempData["Error"] = "Failed to analyze the uploaded image. Error: " + ex.Message;
                return RedirectToAction("Upload");
            }

            // Pass model to view
            return View("Result", result);
        }
    }
}
