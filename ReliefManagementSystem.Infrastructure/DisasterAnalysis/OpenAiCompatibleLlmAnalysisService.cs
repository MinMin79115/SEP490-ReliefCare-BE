using Microsoft.Extensions.Options;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReliefManagementSystem.Infrastructure.DisasterAnalysis
{
    public class OpenAiCompatibleLlmAnalysisService : ILlmAnalysisService
    {
        private const string PromptVersion = "disaster-analysis-v4-numeric-first";

        private readonly HttpClient _httpClient;
        private readonly LlmProviderSettings _settings;

        public OpenAiCompatibleLlmAnalysisService(HttpClient httpClient, IOptions<LlmProviderSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;

            if (!string.IsNullOrWhiteSpace(_settings.BaseUrl) && _httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
            }

            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            }
        }

        public async Task<LlmDisasterAnalysisResult> AnalyzeRiskAsync(
            CurrentWeatherResult weather,
            WeatherForecastResult forecast,
            string locationName,
            string? requestedDisasterType = null,
            string? additionalContext = null,
            string? requestedModel = null,
            CancellationToken cancellationToken = default)
        {
            var model = !string.IsNullOrWhiteSpace(requestedModel)
                ? requestedModel.Trim()
                : _settings.DefaultModel;

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new InvalidOperationException("LlmProvider:ApiKey is missing.");
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidOperationException("No LLM model was provided. Set LlmProvider:DefaultModel or pass request.model.");
            }

            var payload = new
            {
                model,
                temperature = _settings.Temperature,
                max_tokens = _settings.MaxTokens,
                messages = new object[]
                {
                    new { role = "system", content = BuildSystemPrompt() },
                    new { role = "user", content = BuildUserPrompt(weather, forecast, locationName, requestedDisasterType, additionalContext) }
                }
            };

            using var response = await _httpClient.PostAsJsonAsync(
                NormalizePath(_settings.ChatCompletionsPath),
                payload,
                cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(body);
            var content = ExtractAssistantContent(doc.RootElement);
            var jsonContent = ExtractJsonObject(content);

            var parsed = JsonSerializer.Deserialize<LlmJsonResponse>(jsonContent, JsonOptions())
                ?? throw new InvalidOperationException("LLM returned an empty JSON response.");

            return new LlmDisasterAnalysisResult
            {
                ProviderName = string.IsNullOrWhiteSpace(_settings.ProviderName) ? "OpenAI-Compatible" : _settings.ProviderName,
                ModelUsed = model,
                PromptVersion = PromptVersion,
                AnalyzedAt = DateTime.UtcNow,
                Summary = parsed.Summary ?? string.Empty,
                DetailedAnalysis = parsed.DetailedAnalysis ?? string.Empty,
                Recommendations = parsed.Recommendations ?? new List<string>(),
                PotentialScenarios = parsed.PotentialScenarios ?? new List<string>(),
                DetectedConcerns = parsed.DetectedConcerns ?? new List<string>(),
                RawResponse = jsonContent
            };
        }

        private static string BuildSystemPrompt()
        {
            return """
You are a disaster risk analyst supporting relief coordination in Vietnam.
You receive current weather data and a 14-day weather forecast.

Your job:
1. Write a short, natural Vietnamese summary for operators.
2. Interpret the 14-day forecast by highlighting the periods or days that deserve attention.
3. Explain operational concerns in a practical, non-alarmist way based directly on quantitative weather signals.
4. Provide 3-5 short, actionable recommendations.
5. Suggest likely concern patterns if the weather evolves unfavorably.

Rules:
- Always answer in Vietnamese, with natural wording suitable for a human-facing report.
- Infer risk primarily from quantitative signals: precipitation amount, precipitation probability, precipitation cover, wind speed, wind gust, humidity, visibility, pressure, cloud cover, severerisk, CAPE, CIN, and multi-day trends.
- Treat vendor text fields like condition/conditions/description/precipTypes as weak metadata only, not primary evidence.
- Do not quote or paraphrase vendor weather descriptions unless absolutely necessary to clarify precipitation form.
- If you state a concern, tie it to measurable signals or multi-day trends.
- If the user requested a specific disaster type, focus on that type.
- If the requested type cannot be directly predicted from weather alone, say so clearly and cautiously.
- Do not present the result as an official scientific prediction. This is only an AI-assisted operational interpretation of weather data.
- Return pure JSON only with this shape:
{
  "summary": "string",
  "detailedAnalysis": "string",
  "recommendations": ["string"],
  "potentialScenarios": ["string"],
  "detectedConcerns": ["string"]
}
""";
        }

        private static string BuildUserPrompt(
            CurrentWeatherResult weather,
            WeatherForecastResult forecast,
            string locationName,
            string? requestedDisasterType = null,
            string? additionalContext = null)
        {
            var forecastSummary = BuildForecastSummary(forecast);
            var context = string.IsNullOrWhiteSpace(additionalContext) ? "Không có" : additionalContext;
            var target = string.IsNullOrWhiteSpace(requestedDisasterType) ? "Không chỉ định cụ thể" : requestedDisasterType;

            return $"""
## Dữ liệu phân tích
- Khu vực: {locationName}
- Loại rủi ro muốn tập trung: {target}
- Bối cảnh bổ sung: {context}
- Số liệu thời tiết hiện tại: nhiệt độ {weather.TemperatureC:0.##}°C, gió {weather.WindKph:0.##} km/h, mưa {weather.PrecipMm:0.##} mm, tầm nhìn {weather.VisibilityKm:0.##} km, độ ẩm {weather.Humidity}%

## Tóm tắt forecast 14 ngày
{forecastSummary}

Hãy suy luận rủi ro chủ yếu từ số liệu forecast và xu hướng nhiều ngày. Không dựa chủ yếu vào các mô tả text của nhà cung cấp thời tiết. Ưu tiên diễn giải xu hướng theo từng giai đoạn và ngày nổi bật. Không cần lặp lại toàn bộ số liệu. Hãy trả JSON đúng schema đã yêu cầu.
""";
        }

        private static string BuildForecastSummary(WeatherForecastResult forecast)
        {
            if (forecast.Days.Count == 0)
            {
                return "- Không có dữ liệu forecast 14 ngày.";
            }

            var ordered = forecast.Days.OrderBy(x => x.Date).ToList();
            var totalPrecip = ordered.Sum(x => x.PrecipMm);
            var peakDay = ordered.OrderByDescending(x => x.PrecipMm).ThenByDescending(x => x.PrecipProbability).First();
            var firstWindow = ordered.Take(3).ToList();
            var midWindow = ordered.Skip(3).Take(4).ToList();
            var lateWindow = ordered.Skip(7).Take(7).ToList();
            var rainyDays5 = ordered.Count(x => x.PrecipMm >= 5);
            var rainyDays10 = ordered.Count(x => x.PrecipMm >= 10);
            var highProbDays = ordered.Count(x => x.PrecipProbability >= 70);
            var wideCoverDays = ordered.Count(x => x.PrecipCover >= 40);
            var maxWindGust = ordered.Max(x => x.WindGustKph);
            var minVisibility = ordered.Min(x => x.VisibilityKm);
            var maxSevereRisk = ordered.Max(x => x.SevereRisk);
            var maxCape = ordered.Max(x => x.Cape);
            var longestWetStreak = CountLongestWetStreak(ordered);
            var maxRolling3DayRain = MaxRollingPrecip(ordered, 3);
            var attentionDays = ordered
                .Select(x => new
                {
                    Day = x,
                    Score = x.PrecipMm + (x.PrecipProbability / 10.0) + (x.PrecipCover / 10.0) + (x.WindGustKph / 20.0) + (x.SevereRisk / 10.0)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Day.PrecipMm)
                .Take(5)
                .Select(x => x.Day)
                .ToList();

            return string.Join("\n", new[]
            {
                $"- Địa điểm forecast: {(string.IsNullOrWhiteSpace(forecast.ResolvedAddress) ? "Không rõ" : forecast.ResolvedAddress)}",
                $"- Tổng lượng mưa 14 ngày: {totalPrecip:0.##} mm",
                $"- Ngày mưa đáng chú ý nhất hiện là {peakDay.Date:dd/MM} với khoảng {peakDay.PrecipMm:0.##} mm và xác suất {peakDay.PrecipProbability:0.##}%",
                $"- Tổng mưa giai đoạn 1-3 ngày: {firstWindow.Sum(x => x.PrecipMm):0.##} mm",
                $"- Tổng mưa giai đoạn 4-7 ngày: {midWindow.Sum(x => x.PrecipMm):0.##} mm",
                $"- Tổng mưa giai đoạn 8-14 ngày: {lateWindow.Sum(x => x.PrecipMm):0.##} mm",
                $"- Số ngày có mưa >= 5 mm: {rainyDays5}; >= 10 mm: {rainyDays10}",
                $"- Số ngày có xác suất mưa >= 70%: {highProbDays}; độ phủ mưa >= 40%: {wideCoverDays}",
                $"- Chuỗi ngày ẩm/mưa dài nhất: {longestWetStreak} ngày",
                $"- Tổng mưa dồn 3 ngày lớn nhất: {maxRolling3DayRain:0.##} mm",
                $"- Gió giật lớn nhất: {maxWindGust:0.##} km/h; tầm nhìn thấp nhất: {minVisibility:0.##} km",
                $"- Severerisk cao nhất: {maxSevereRisk:0.##}; CAPE cao nhất: {maxCape:0.##}",
                "- Các ngày cần ưu tiên xem xét theo tín hiệu số liệu:",
                string.Join("\n", attentionDays.Select(x =>
                    $"  - {x.Date:dd/MM}: mưa {x.PrecipMm:0.##} mm, xác suất {x.PrecipProbability:0.##}%, độ phủ {x.PrecipCover:0.##}%, gió {x.WindSpeedKph:0.##} km/h, gust {x.WindGustKph:0.##} km/h, độ ẩm {x.Humidity:0.##}%, tầm nhìn {x.VisibilityKm:0.##} km, áp suất {x.Pressure:0.##}, severerisk {x.SevereRisk:0.##}, CAPE {x.Cape:0.##}, CIN {x.Cin:0.##}"))
            });
        }

        private static int CountLongestWetStreak(IReadOnlyCollection<ForecastDayResult> days)
        {
            var current = 0;
            var max = 0;

            foreach (var day in days.OrderBy(x => x.Date))
            {
                if (day.PrecipMm >= 1 || day.PrecipProbability >= 60)
                {
                    current++;
                    max = Math.Max(max, current);
                }
                else
                {
                    current = 0;
                }
            }

            return max;
        }

        private static double MaxRollingPrecip(IReadOnlyList<ForecastDayResult> days, int window)
        {
            if (days.Count == 0 || window <= 0)
            {
                return 0;
            }

            double max = 0;
            for (var i = 0; i <= days.Count - window; i++)
            {
                var total = 0d;
                for (var j = 0; j < window; j++)
                {
                    total += days[i + j].PrecipMm;
                }
                max = Math.Max(max, total);
            }

            return max;
        }

        private static string ExtractAssistantContent(JsonElement root)
        {
            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("LLM response does not contain choices.");
            }

            var message = choices[0].GetProperty("message");
            if (!message.TryGetProperty("content", out var contentElement))
            {
                throw new InvalidOperationException("LLM response does not contain message content.");
            }

            return contentElement.ValueKind switch
            {
                JsonValueKind.String => contentElement.GetString() ?? string.Empty,
                JsonValueKind.Array => string.Concat(contentElement.EnumerateArray()
                    .Where(x => x.TryGetProperty("type", out var type) && type.GetString() == "text")
                    .Select(x => x.TryGetProperty("text", out var text) ? text.GetString() : null)
                    .Where(x => !string.IsNullOrWhiteSpace(x))),
                _ => contentElement.GetRawText()
            };
        }

        private static string ExtractJsonObject(string content)
        {
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');

            if (start < 0 || end <= start)
            {
                throw new InvalidOperationException($"LLM did not return valid JSON. Raw content: {content}");
            }

            return content[start..(end + 1)];
        }

        private static string NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "chat/completions";
            }

            return path.TrimStart('/');
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private class LlmJsonResponse
        {
            public string? Summary { get; set; }
            public string? DetailedAnalysis { get; set; }
            public List<string>? Recommendations { get; set; }
            public List<string>? PotentialScenarios { get; set; }
            public List<string>? DetectedConcerns { get; set; }
        }
    }
}
