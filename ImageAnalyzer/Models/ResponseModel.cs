namespace ImageAnalyzer.Models
{
    public class ResponseModel
    {
        public List<string> Captions { get; set; } = new();
        public List<string> Alts { get; set; } = new();
    }
}
