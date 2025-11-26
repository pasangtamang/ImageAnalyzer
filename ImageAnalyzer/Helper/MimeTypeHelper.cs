namespace ImageAnalyzer.Helper
{
    public static class MimeTypeHelper
    {
        private static readonly Dictionary<string, string> _mimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg",  "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png",  "image/png" },
        { ".gif",  "image/gif" },
        { ".bmp",  "image/bmp" },
        { ".webp", "image/webp" },
        { ".tiff", "image/tiff" },
        { ".svg",  "image/svg+xml" }
    };

        public static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null)
                return "application/octet-stream";

            return _mimeTypes.TryGetValue(extension, out var mime)
                ? mime
                : "application/octet-stream";
        }
    }
}
