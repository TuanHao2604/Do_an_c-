using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartTourApp.Domain.Interfaces;
using SmartTourApp.Domain.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SmartTourApp.Application.Services;

public class AiGuideService : IAiGuideService
{
    private readonly IAppDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public AiGuideService(IAppDbContext db, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _db = db;
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _apiKey = configuration["OpenAI:ApiKey"] ?? "";
        _model = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
    }

    public async Task<string> GetGuideDescriptionAsync(Guid poiId, string languageCode = "vi")
    {
        var content = await _db.PoiContents
            .Include(c => c.Poi).ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.PoiId == poiId && c.LanguageCode == languageCode);

        if (content is null)
            return "Không tìm thấy thông tin POI.";

        var prompt = $"""
            Bạn là hướng dẫn viên du lịch ảo. Hãy giới thiệu về địa điểm sau đây một cách sinh động và hấp dẫn:

            Tên: {content.Name}
            Loại: {content.Poi.Category?.Name}
            Địa chỉ: {content.Address}
            Mô tả: {content.Description}

            Hãy trả lời bằng {(languageCode == "vi" ? "tiếng Việt" : "English")}, khoảng 200 từ.
            """;

        if (string.IsNullOrEmpty(_apiKey))
        {
            // Fallback when no API key configured
            return $"🏛️ Chào mừng bạn đến với **{content.Name}**!\n\n" +
                   $"{content.Description ?? "Đây là một địa điểm tuyệt vời để khám phá."}\n\n" +
                   $"📍 Địa chỉ: {content.Address ?? "Đang cập nhật"}\n" +
                   $"🏷️ Danh mục: {content.Poi.Category?.Name ?? "Chung"}";
        }

        try
        {
            var requestBody = new
            {
                model = _model,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 500,
                temperature = 0.7
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }
        catch (Exception ex)
        {
            return $"⚠️ Không thể kết nối AI. Lỗi: {ex.Message}\n\n" +
                   $"📌 {content.Name}: {content.Description}";
        }
    }
}
