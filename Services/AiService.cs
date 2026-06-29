using System.Text.Json;

namespace AustimAPI.Services
{
    public class AIService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public AIService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<QuestionAIResult?> AnalyzeQuestions(object payload)
        {
            try
            {
                var client = _httpFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var url = _config["AISettings:QuestionsUrl"]
                    ?? "https://ammarhisham22-mchat-autism-screening.hf.space/predict";

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new System.Net.Http.StringContent(
                    json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                    return null;

                var resultJson = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
                var root = doc.RootElement;

                var result = new QuestionAIResult();

                if (root.TryGetProperty("overall_assessment", out var assessment))
                {
                    if (assessment.TryGetProperty("risk_level_text", out var riskText))
                        result.RiskLevel = riskText.GetString() ?? "Unknown";

                    if (assessment.TryGetProperty("risk_probability", out var prob))
                        result.RiskScorePercentage = (float)Math.Round(prob.GetDouble() * 100, 2);

                    if (assessment.TryGetProperty("medical_interpretation", out var interp))
                        result.FinalResult = interp.GetString() ?? "";
                }

                result.RawJson = resultJson;
                return result;
            }
            catch
            {
                return null;
            }
        }

        // ✅ تعديل كامل: نقرأ الـ shape الحقيقي من analysis_result
        public async Task<VideoAIResult?> AnalyzeVideo(string filePath)
        {
            try
            {
                var client = _httpFactory.CreateClient();

                // الفيديو قد يستغرق عدة دقائق
                client.Timeout = TimeSpan.FromMinutes(5);

                var url = _config["AISettings:VideoUrl"]
                    ?? "https://ammarhisham22-neuromotion-ads.hf.space/api/v1/predict";

                // إرسال الفيديو كـ multipart/form-data
                using var form = new MultipartFormDataContent();

                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                using var fileContent = new StreamContent(fileStream);

                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");

                // اسم الحقل المطلوب بواسطة FastAPI
                form.Add(fileContent, "video", Path.GetFileName(filePath));

                var response = await client.PostAsync(url, form);

                // فى حالة وجود Error من الـ AI
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"AI Error: {(int)response.StatusCode}");
                    Console.WriteLine(error);

                    return null;
                }

                var resultJson = await response.Content.ReadAsStringAsync();

                Console.WriteLine("AI Response:");
                Console.WriteLine(resultJson);

                using var doc = JsonDocument.Parse(resultJson);
                var root = doc.RootElement;

                var result = new VideoAIResult
                {
                    RawJson = resultJson
                };

                if (root.TryGetProperty("analysis_result", out var analysis))
                {
                    if (analysis.TryGetProperty("risk_score_percentage", out var riskScore))
                        result.RiskScorePercentage = (float)riskScore.GetDouble();

                    if (analysis.TryGetProperty("overall_confidence", out var confidence))
                        result.OverallConfidence =
                            (float)Math.Round(confidence.GetDouble() * 100, 2);

                    
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Video AI Exception:");
                Console.WriteLine(ex.ToString());

                return null;
            }
        }
    }

    public class QuestionAIResult
    {
        public string RiskLevel { get; set; } = "Unknown";
        public float RiskScorePercentage { get; set; } = 0;
        public string FinalResult { get; set; } = "";
        public string RawJson { get; set; } = "";
    }


    public class VideoAIResult
    {
        public float? RiskScorePercentage { get; set; }
        public float? OverallConfidence { get; set; }
        public string RawJson { get; set; } = "";
    }
}