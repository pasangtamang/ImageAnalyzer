using System.Text;
using System.Text.Json;

namespace ImageAnalyzer.Services
{
    public class GeminiService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public GeminiService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(List<string> captions, List<string> alts)>
            AnalyzeImageAsync(byte[] imageBytes, string mimeType, string language = "English")
        {
            string apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Gemini API Key missing");

            var http = _httpClientFactory.CreateClient();

            string base64Image = Convert.ToBase64String(imageBytes);

            var requestBody = new
            {
                contents = new[]
                {
                new {
                    parts = new object[]
                    {
                        new { text =
                            $"Analyze this image and return:" +
                            $"\n - 5 caption options" +
                            $"\n - 5 alt text options" +
                            $"\n - Language: {language}" +
                            $"\nRespond in JSON only: {{ \"captions\": [...], \"alts\": [...] }}"
                        },
                        new {
                            inline_data = new {
                                mime_type = mimeType,
                                data = base64Image
                            }
                        }
                    }
                }
            }
            };

            // 🔥 FIXED URL (no more v1beta)
            string url =
                $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var response = await http.PostAsync(
                url,
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API Error: {error}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            var textResponse = json
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            textResponse = textResponse.Replace("```json", "").Replace("```", "");

            var resultJson = JsonDocument.Parse(textResponse).RootElement;

            var captions = resultJson.GetProperty("captions")
                .EnumerateArray().Select(x => x.GetString()).ToList();

            var alts = resultJson.GetProperty("alts")
                .EnumerateArray().Select(x => x.GetString()).ToList();

            return (captions, alts);
        }
    }
}
