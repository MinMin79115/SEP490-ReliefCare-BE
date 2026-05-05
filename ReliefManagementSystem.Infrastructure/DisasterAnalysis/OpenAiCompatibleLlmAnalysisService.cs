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
            var content = ExtractAssistantContent(doc.RootElement, body);
            var jsonContent = ExtractJsonObject(content, body);

            var parsed = ParseLlmJsonResponse(jsonContent);
            EnsureFloodOnlyDefaults(parsed, forecast);

            if (!parsed.HasUsableContent())
            {
                parsed = ParseNestedLlmJsonResponse(jsonContent);
                EnsureFloodOnlyDefaults(parsed, forecast);
            }

            if (!parsed.HasUsableContent())
            {
                throw new LlmAnalysisException("LLM returned JSON but it did not match the expected schema.", jsonContent);
            }

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
1. Write a natural Vietnamese flood-risk narrative for relief operators.
2. Explain the 14-day forecast as a connected argument, not as terse labels or isolated bullet points.
3. Focus on why the flood risk changes by period, especially rainfall accumulation, peak rainfall days, local drainage sensitivity, and uncertainty.
4. Provide practical recommendations, but each recommendation must include the reason behind it.
5. Suggest likely flood scenarios if the weather evolves unfavorably, using full explanatory sentences.

Rules:
- Always answer in Vietnamese, with natural wording suitable for a human-facing report.
- Infer risk primarily from quantitative signals: precipitation amount, precipitation probability, precipitation cover, wind speed, wind gust, humidity, visibility, pressure, cloud cover, severerisk, CAPE, CIN, and multi-day trends.
- Treat vendor text fields like condition/conditions/description/precipTypes as weak metadata only, not primary evidence.
- Do not quote or paraphrase vendor weather descriptions unless absolutely necessary to clarify precipitation form.
- If you state a concern, tie it to measurable signals or multi-day trends.
- Focus only on flooding: heavy rain flooding, flash flooding, river flooding, low-lying area inundation, drainage overload, and flood-related transport disruption.
- Do not include standalone thunderstorm, lightning, heat, drought, or wind risk unless it directly worsens flood response or flood access.
- Do not present the result as an official scientific prediction. This is only an AI-assisted operational interpretation of weather data.
- Return pure JSON only. Do not wrap it in Markdown.
- Use exactly these property names. Do not translate them and do not use snake_case:
- All five properties are required. Strings must be non-empty. Arrays must contain at least one non-empty string.
- Do not write terse items like "17/05 - moderate" or "flooding: low". Every array item must be a complete Vietnamese sentence with context and reasoning.
- "summary" should be one substantial paragraph of 3-5 sentences.
- "detailedAnalysis" should be the longest field: 3-5 connected paragraphs in one string. Cover early period, middle period, late period, peak rain day, confidence/uncertainty, and operational meaning for nearby communities.
- "recommendations" should contain 4-6 complete sentences. Each item should say what to do and why it matters for flood response.
- "potentialScenarios" should contain 3-5 complete scenario sentences. Each item should describe a flood-related condition, likely impact, and where/when it matters.
- "detectedConcerns" should contain 3-5 complete concern sentences, not labels. Explain the measured signal behind each concern.
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

Hãy suy luận rủi ro lũ/ngập từ số liệu forecast và xu hướng nhiều ngày. Viết như một báo cáo phân tích cho điều phối cứu trợ, có lập luận liền mạch và giải thích vì sao mỗi tín hiệu số liệu làm tăng hoặc giảm nguy cơ. Không trả các gạch đầu dòng ngắn hoặc nhãn rủi ro trống ngữ cảnh. Không dựa chủ yếu vào mô tả text của nhà cung cấp thời tiết. Hãy trả JSON đúng schema đã yêu cầu, nhưng nội dung trong từng field phải dài, tự nhiên và giàu suy luận.
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

        private static string ExtractAssistantContent(JsonElement root, string rawResponse)
        {
            if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                throw new LlmAnalysisException("LLM response does not contain choices.", rawResponse);
            }

            var message = choices[0].GetProperty("message");
            if (!message.TryGetProperty("content", out var contentElement))
            {
                throw new LlmAnalysisException("LLM response does not contain message content.", rawResponse);
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

        private static string ExtractJsonObject(string content, string rawResponse)
        {
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');

            if (start < 0 || end <= start)
            {
                throw new LlmAnalysisException($"LLM did not return valid JSON. Raw content: {content}", rawResponse);
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

        private static LlmJsonResponse ParseLlmJsonResponse(string jsonContent)
        {
            var parsed = JsonSerializer.Deserialize<LlmJsonResponse>(jsonContent, JsonOptions())
                ?? throw new InvalidOperationException("LLM returned an empty JSON response.");

            if (parsed.HasUsableContent())
            {
                return parsed;
            }

            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            return new LlmJsonResponse
            {
                Summary = GetString(root, "summary", "overall_assessment", "key_assessment")
                    ?? GetNestedString(root, "overall_assessment", "summary")
                    ?? GetNestedString(root, "final_assessment", "primary_concern"),
                DetailedAnalysis = GetString(root, "detailedAnalysis")
                    ?? BuildNarrativeAnalysis(root)
                    ?? BuildObjectSection(root, "phase_analysis", "period", "risk_level", "interpretation")
                    ?? BuildObjectSection(root, "phase_assessment", "period", "risk_level", "reasoning")
                    ?? BuildObjectSectionFromObject(root, "hazard_breakdown", "risk_level", "trend", "rationale")
                    ?? BuildStringSection(root, "key_drivers")
                    ?? BuildStringSection(root, "likely_impacts"),
                Recommendations = GetStringList(root, "recommendations", "recommended_actions")
                    ?? GetStringList(root, "operational_recommendations")
                    ?? GetNestedStringList(root, "operational_implications", "recommended_actions"),
                PotentialScenarios = FilterFloodRelated(GetStringList(root, "potentialScenarios", "watch_triggers", "priority_locations"))
                    ?? GetNotableDaySummaries(root)
                    ?? GetWatchTriggerSummaries(root)
                    ?? GetNestedStringList(root, "alert_level", "trigger_conditions_for_escalation"),
                DetectedConcerns = FilterFloodRelated(GetStringList(root, "detectedConcerns", "drivers_increasing_risk", "drivers_reducing_risk", "key_uncertainties"))
                    ?? FilterFloodRelated(GetNestedStringList(root, "overall_assessment", "main_hazards"))
                    ?? GetHazardConcernSummaries(root)
            };
        }

        private static void EnsureFloodOnlyDefaults(LlmJsonResponse parsed, WeatherForecastResult forecast)
        {
            if (parsed.PotentialScenarios == null || parsed.PotentialScenarios.Count == 0)
            {
                parsed.PotentialScenarios = BuildFloodScenarios(forecast);
            }

            if (parsed.Recommendations == null || parsed.Recommendations.Count == 0)
            {
                parsed.Recommendations = BuildFloodRecommendations(forecast);
            }

            if (parsed.DetectedConcerns == null || parsed.DetectedConcerns.Count == 0)
            {
                parsed.DetectedConcerns = BuildFloodConcerns(forecast);
            }
        }

        private static List<string> BuildFloodScenarios(WeatherForecastResult forecast)
        {
            var peakDay = forecast.Days
                .OrderByDescending(x => x.PrecipMm)
                .ThenByDescending(x => x.PrecipProbability)
                .FirstOrDefault();

            if (peakDay == null)
            {
                return new List<string> { "Nếu xuất hiện mưa lớn cục bộ, cần theo dõi nguy cơ ngập nhanh tại các điểm trũng và khu thoát nước yếu." };
            }

            return new List<string>
            {
                $"Nếu mưa tập trung quanh ngày {peakDay.Date:dd/MM}, các khu vực trũng thấp hoặc thoát nước kém có thể xuất hiện ngập cục bộ.",
                "Nếu mưa thực tế kéo dài hơn dự báo, việc tiếp cận khu dân cư và tuyến đường thấp có thể bị chậm do ngập hoặc mặt đường trơn."
            };
        }

        private static List<string> BuildFloodConcerns(WeatherForecastResult forecast)
        {
            var concerns = new List<string>();
            var totalPrecip = forecast.Days.Sum(x => x.PrecipMm);
            var maxRolling3DayRain = MaxRollingPrecip(forecast.Days.OrderBy(x => x.Date).ToList(), 3);

            if (forecast.Days.Any(x => x.PrecipMm >= 20)) concerns.Add("Có ngày mưa nổi bật, cần theo dõi nguy cơ ngập cục bộ tại vùng trũng thấp.");
            if (maxRolling3DayRain >= 25) concerns.Add("Mưa dồn trong vài ngày có thể làm tăng tải hệ thống thoát nước cục bộ.");
            if (totalPrecip >= 30) concerns.Add("Tổng mưa toàn kỳ đủ để duy trì mức theo dõi lũ/ngập trong phạm vi hoạt động của trạm.");

            return concerns.Count == 0
                ? new List<string> { "Chưa có tín hiệu lũ lớn, nhưng vẫn cần theo dõi các điểm ngập quen thuộc nếu mưa tập trung trong thời gian ngắn." }
                : concerns;
        }

        private static List<string> BuildFloodRecommendations(WeatherForecastResult forecast)
        {
            var peakDay = forecast.Days
                .OrderByDescending(x => x.PrecipMm)
                .ThenByDescending(x => x.PrecipProbability)
                .FirstOrDefault();

            return new List<string>
            {
                "Theo dõi cập nhật dự báo mưa và cảnh báo ngập tại các điểm trũng thấp, cống thoát nước nhỏ và khu dân cư dễ ngập.",
                peakDay == null
                    ? "Chuẩn bị phương án di chuyển thay thế nếu mưa thực tế tập trung trong thời gian ngắn."
                    : $"Ưu tiên rà soát các điểm ngập quen thuộc trước ngày {peakDay.Date:dd/MM}, là ngày có tín hiệu mưa đáng chú ý nhất.",
                "Kiểm tra tuyến tiếp cận trạm và khu dân cư gần trạm để tránh chậm điều phối khi xuất hiện ngập cục bộ."
            };
        }

        private static LlmJsonResponse ParseNestedLlmJsonResponse(string jsonContent)
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var candidates = new Queue<JsonElement>();
            candidates.Enqueue(doc.RootElement);

            while (candidates.Count > 0)
            {
                var current = candidates.Dequeue();
                if (current.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var parsed = new LlmJsonResponse
                {
                    Summary = GetString(current, "summary", "overall_assessment", "key_assessment")
                        ?? GetNestedString(current, "overall_assessment", "summary")
                        ?? GetNestedString(current, "final_assessment", "primary_concern"),
                    DetailedAnalysis = GetString(current, "detailedAnalysis")
                        ?? BuildNarrativeAnalysis(current)
                        ?? BuildObjectSection(current, "phase_analysis", "period", "risk_level", "interpretation")
                        ?? BuildObjectSection(current, "phase_assessment", "period", "risk_level", "reasoning")
                        ?? BuildObjectSectionFromObject(current, "hazard_breakdown", "risk_level", "trend", "rationale")
                        ?? BuildStringSection(current, "key_drivers")
                        ?? BuildStringSection(current, "likely_impacts"),
                    Recommendations = GetStringList(current, "recommendations", "recommended_actions")
                        ?? GetStringList(current, "operational_recommendations")
                        ?? GetNestedStringList(current, "operational_implications", "recommended_actions"),
                    PotentialScenarios = FilterFloodRelated(GetStringList(current, "potentialScenarios", "watch_triggers", "priority_locations"))
                        ?? GetNotableDaySummaries(current)
                        ?? GetWatchTriggerSummaries(current)
                        ?? GetNestedStringList(current, "alert_level", "trigger_conditions_for_escalation"),
                    DetectedConcerns = FilterFloodRelated(GetStringList(current, "detectedConcerns", "drivers_increasing_risk", "drivers_reducing_risk", "key_uncertainties"))
                        ?? FilterFloodRelated(GetNestedStringList(current, "overall_assessment", "main_hazards"))
                        ?? GetHazardConcernSummaries(current)
                };

                if (parsed.HasUsableContent())
                {
                    return parsed;
                }

                foreach (var property in current.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        candidates.Enqueue(property.Value);
                    }
                }
            }

            return new LlmJsonResponse();
        }

        private static string? GetString(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }
            }

            return null;
        }

        private static string? GetNestedString(JsonElement root, string objectName, string propertyName)
        {
            return root.TryGetProperty(objectName, out var nested) && nested.ValueKind == JsonValueKind.Object
                ? GetString(nested, propertyName)
                : null;
        }

        private static List<string>? GetStringList(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (!root.TryGetProperty(name, out var value))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.Array)
                {
                    return value.EnumerateArray()
                        .Where(x => x.ValueKind == JsonValueKind.String)
                        .Select(x => x.GetString())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!)
                        .ToList();
                }

                if (value.ValueKind == JsonValueKind.String)
                {
                    var text = value.GetString();
                    return string.IsNullOrWhiteSpace(text) ? new List<string>() : new List<string> { text };
                }
            }

            return null;
        }

        private static List<string>? GetNestedStringList(JsonElement root, string objectName, string propertyName)
        {
            return root.TryGetProperty(objectName, out var nested) && nested.ValueKind == JsonValueKind.Object
                ? GetStringList(nested, propertyName)
                : null;
        }

        private static List<string>? GetObjectSummaryList(JsonElement root, string propertyName, params string[] fields)
        {
            if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var result = value.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.Object)
                .Select(x => string.Join(" - ", fields
                    .Select(field => x.TryGetProperty(field, out var fieldValue) && fieldValue.ValueKind == JsonValueKind.String
                        ? fieldValue.GetString()
                        : null)
                    .Where(text => !string.IsNullOrWhiteSpace(text))))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();

            return result.Count == 0 ? null : result;
        }

        private static List<string>? GetNotableDaySummaries(JsonElement root)
        {
            if (!root.TryGetProperty("notable_days", out var value) || value.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var result = value.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.Object)
                .Where(day => IsFloodRelated(GetString(day, "primary_concern"))
                    || IsFloodRelated(GetString(day, "reason"))
                    || IsFloodRelated(GetString(day, "main_signal"))
                    || IsFloodRelated(GetString(day, "interpretation")))
                .Select(day =>
                {
                    var date = GetString(day, "date");
                    var riskLevel = FormatRiskLevel(GetString(day, "risk_level"));
                    var reason = GetString(day, "reason") ?? GetString(day, "interpretation") ?? GetString(day, "main_signal");
                    var concern = FormatCodeLabel(GetString(day, "primary_concern"));

                    var parts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(date)) parts.Add($"Ngày {date}");
                    if (!string.IsNullOrWhiteSpace(riskLevel)) parts.Add($"rủi ro {riskLevel}");
                    if (!string.IsNullOrWhiteSpace(concern)) parts.Add($"mối quan tâm chính: {concern}");

                    var sentence = string.Join(", ", parts);
                    if (!string.IsNullOrWhiteSpace(reason))
                    {
                        sentence = string.IsNullOrWhiteSpace(sentence) ? reason : $"{sentence}. {reason}";
                    }

                    return sentence;
                })
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();

            return result.Count == 0 ? null : result;
        }

        private static List<string>? GetWatchTriggerSummaries(JsonElement root)
        {
            if (!root.TryGetProperty("watch_triggers", out var value) || value.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var result = value.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.Object)
                .Select(trigger =>
                {
                    var condition = GetString(trigger, "condition");
                    var action = GetString(trigger, "suggested_action");

                    if (string.IsNullOrWhiteSpace(condition))
                    {
                        return action;
                    }

                    return string.IsNullOrWhiteSpace(action)
                        ? condition
                        : $"Nếu {condition.ToLowerInvariant()}, {action}";
                })
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => text!)
                .ToList();

            return result.Count == 0 ? null : result;
        }

        private static string? BuildStringSection(JsonElement root, string propertyName)
        {
            var lines = GetStringList(root, propertyName);
            return lines == null || lines.Count == 0 ? null : string.Join("\n", lines);
        }

        private static string? BuildNarrativeAnalysis(JsonElement root)
        {
            var sections = new List<string?>
            {
                GetString(root, "main_reasoning"),
                BuildStringObjectSection(root, "time_windows"),
                GetString(root, "drivers_increasing_risk"),
                GetString(root, "drivers_reducing_risk"),
                GetString(root, "operational_implications"),
                GetString(root, "priority_locations"),
                GetString(root, "watch_triggers"),
                GetString(root, "key_uncertainties")
            };

            var text = sections
                .Where(section => !string.IsNullOrWhiteSpace(section))
                .Select(section => section!)
                .ToList();

            return text.Count == 0 ? null : string.Join("\n\n", text);
        }

        private static string? BuildStringObjectSection(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var sections = value.EnumerateObject()
                .Where(property => property.Value.ValueKind == JsonValueKind.String)
                .Select(property => property.Value.GetString())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => text!)
                .ToList();

            return sections.Count == 0 ? null : string.Join("\n\n", sections);
        }

        private static List<string>? GetHazardConcernSummaries(JsonElement root)
        {
            if (!root.TryGetProperty("hazard_breakdown", out var value) || value.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var result = value.EnumerateObject()
                .Where(property => property.Value.ValueKind == JsonValueKind.Object)
                .Where(property => IsFloodRelated(property.Name))
                .Select(property =>
                {
                    var hazard = FormatCodeLabel(property.Name) ?? property.Name;
                    var riskLevel = FormatRiskLevel(GetString(property.Value, "risk_level"));
                    var trend = GetString(property.Value, "trend");
                    var rationale = GetString(property.Value, "rationale");

                    var sentence = string.IsNullOrWhiteSpace(riskLevel)
                        ? hazard
                        : $"{hazard}: rủi ro {riskLevel}";

                    if (!string.IsNullOrWhiteSpace(trend))
                    {
                        sentence += $", xu hướng {trend}";
                    }

                    if (!string.IsNullOrWhiteSpace(rationale))
                    {
                        sentence += $". {rationale}";
                    }

                    return sentence;
                })
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();

            return result.Count == 0 ? null : result;
        }

        private static List<string>? FilterFloodRelated(List<string>? values)
        {
            var result = values?
                .Where(IsFloodRelated)
                .ToList();

            return result == null || result.Count == 0 ? null : result;
        }

        private static bool IsFloodRelated(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value.Trim().ToLowerInvariant();
            return normalized.Contains("flood")
                || normalized.Contains("rain")
                || normalized.Contains("precip")
                || normalized.Contains("inundation")
                || normalized.Contains("drainage")
                || normalized.Contains("landslide")
                || normalized.Contains("slope")
                || normalized.Contains("lụt")
                || normalized.Contains("lũ")
                || normalized.Contains("ngập")
                || normalized.Contains("mưa");
        }

        private static string? FormatRiskLevel(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant() switch
                {
                    "thap" => "thấp",
                    "trung_binh" => "trung bình",
                    "cao" => "cao",
                    "thap_den_trung_binh" => "thấp đến trung bình",
                    "trung_binh_den_cao" => "trung bình đến cao",
                    "trung_binh_den_cao_cuc_bo" => "trung bình đến cao cục bộ",
                    _ => FormatCodeLabel(value)
                };
        }

        private static string? FormatCodeLabel(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim().Replace('_', ' ').Replace('-', ' ');
        }

        private static string? BuildObjectSection(JsonElement root, string propertyName, params string[] fields)
        {
            var lines = GetObjectSummaryList(root, propertyName, fields);
            return lines == null || lines.Count == 0 ? null : string.Join("\n", lines);
        }

        private static string? BuildObjectSectionFromObject(JsonElement root, string propertyName, params string[] fields)
        {
            var lines = BuildObjectSectionListFromObject(root, propertyName, fields);
            return lines == null || lines.Count == 0 ? null : string.Join("\n", lines);
        }

        private static List<string>? BuildObjectSectionListFromObject(JsonElement root, string propertyName, params string[] fields)
        {
            if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var result = value.EnumerateObject()
                .Where(property => property.Value.ValueKind == JsonValueKind.Object)
                .Select(property => $"{property.Name}: " + string.Join(" - ", fields
                    .Select(field => property.Value.TryGetProperty(field, out var fieldValue) && fieldValue.ValueKind == JsonValueKind.String
                        ? fieldValue.GetString()
                        : null)
                    .Where(text => !string.IsNullOrWhiteSpace(text))))
                .Where(text => !string.IsNullOrWhiteSpace(text) && !text.EndsWith(": ", StringComparison.Ordinal))
                .ToList();

            return result.Count == 0 ? null : result;
        }

        private class LlmJsonResponse
        {
            public string? Summary { get; set; }
            public string? DetailedAnalysis { get; set; }
            public List<string>? Recommendations { get; set; }
            public List<string>? PotentialScenarios { get; set; }
            public List<string>? DetectedConcerns { get; set; }

            public bool HasRequiredSchema()
            {
                return !string.IsNullOrWhiteSpace(Summary)
                    && !string.IsNullOrWhiteSpace(DetailedAnalysis)
                    && HasItems(Recommendations)
                    && HasItems(PotentialScenarios)
                    && HasItems(DetectedConcerns);
            }

            public bool HasUsableContent()
            {
                return !string.IsNullOrWhiteSpace(Summary)
                    || !string.IsNullOrWhiteSpace(DetailedAnalysis)
                    || HasItems(Recommendations)
                    || HasItems(PotentialScenarios)
                    || HasItems(DetectedConcerns);
            }

            private static bool HasItems(List<string>? values)
            {
                return values != null && values.Any(x => !string.IsNullOrWhiteSpace(x));
            }
        }
    }
}
