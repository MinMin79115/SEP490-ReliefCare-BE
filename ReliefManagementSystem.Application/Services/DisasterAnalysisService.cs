using Microsoft.Extensions.Options;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Request;
using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System.Text.Json;

namespace ReliefManagementSystem.Application.Services
{
    public class DisasterAnalysisService : IDisasterAnalysisService
    {
        private readonly IWeatherService _weatherService;
        private readonly IDisasterForecastService _forecastService;
        private readonly ILlmAnalysisService _llmAnalysisService;
        private readonly IUnitOfWork _unitOfWork;

        public DisasterAnalysisService(
            IWeatherService weatherService,
            IDisasterForecastService forecastService,
            ILlmAnalysisService llmAnalysisService,
            IUnitOfWork unitOfWork,
            IOptions<DisasterAnalysisSettings> options)
        {
            _weatherService = weatherService;
            _forecastService = forecastService;
            _llmAnalysisService = llmAnalysisService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AnalyzeDisasterRiskResponse> AnalyzeAsync(
            AnalyzeDisasterRiskRequest request,
            CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);

            var locationName = string.IsNullOrWhiteSpace(request.LocationName)
                ? $"Lat {request.Latitude:0.####}, Lng {request.Longitude:0.####}"
                : request.LocationName.Trim();

            var weather = await _weatherService.GetCurrentWeatherAsync(
                request.Latitude,
                request.Longitude,
                cancellationToken);

            var forecast = await _forecastService.GetFloodForecastAsync(
                request.Latitude,
                request.Longitude,
                14,
                cancellationToken);

            var requestedDisasterType = request.DisasterType;
            var analysisMode = requestedDisasterType.HasValue ? "Focused" : "AutoDetect";

            LlmDisasterAnalysisResult? llmResult = null;
            string? llmError = null;

            try
            {
                llmResult = await _llmAnalysisService.AnalyzeRiskAsync(
                    weather,
                    forecast,
                    locationName,
                    requestedDisasterType?.ToString(),
                    request.AdditionalContext,
                    request.Model,
                    cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                llmError = ex.Message;
            }

            var log = new DisasterAnalysisLog
            {
                DisasterAnalysisLogId = Guid.NewGuid(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                LocationName = locationName,
                DisasterType = requestedDisasterType ?? DisasterType.Flood,
                RequestedModel = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model.Trim(),
                AdditionalContext = request.AdditionalContext,
                WeatherSnapshotJson = JsonSerializer.Serialize(new { current = weather, forecast }),
                HeuristicRiskScore = 0,
                HeuristicRiskLevel = "AI-First",
                AssessmentConfidence = "ModelDependent",
                DataLimitationNote = "Kết quả được AI diễn giải trực tiếp từ dữ liệu thời tiết và forecast, không đi qua heuristic scoring ở backend.",
                TriggerFactorsJson = JsonSerializer.Serialize(Array.Empty<string>()),
                PotentialScenariosJson = JsonSerializer.Serialize(llmResult?.PotentialScenarios ?? BuildFallbackScenarios(forecast)),
                TopThreatsJson = JsonSerializer.Serialize(Array.Empty<string>()),
                LlmProvider = llmResult?.ProviderName,
                LlmModel = llmResult?.ModelUsed,
                PromptVersion = llmResult?.PromptVersion,
                LlmResponseJson = llmResult?.RawResponse,
                ErrorMessage = llmError
            };

            await _unitOfWork.DisasterAnalysisLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AnalyzeDisasterRiskResponse
            {
                AnalysisLogId = log.DisasterAnalysisLogId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                LocationName = locationName,
                AnalysisMode = analysisMode,
                RequestedDisasterType = requestedDisasterType?.ToString(),
                Weather = WeatherSnapshotDto.From(weather),
                Forecast = FloodForecastDto.From(forecast),
                Ai = new AiDisasterNarrativeDto
                {
                    Succeeded = llmResult != null,
                    Provider = llmResult?.ProviderName,
                    Model = llmResult?.ModelUsed,
                    PromptVersion = llmResult?.PromptVersion,
                    AnalyzedAt = llmResult?.AnalyzedAt,
                    RequestedRiskType = requestedDisasterType?.ToString(),
                    Summary = llmResult?.Summary ?? BuildFallbackSummary(requestedDisasterType, forecast),
                    DetailedAnalysis = llmResult?.DetailedAnalysis ?? BuildFallbackDetailedAnalysis(forecast),
                    Recommendations = llmResult?.Recommendations?.ToList() ?? BuildFallbackRecommendations(forecast),
                    PotentialScenarios = llmResult?.PotentialScenarios?.ToList() ?? BuildFallbackScenarios(forecast),
                    DetectedConcerns = llmResult?.DetectedConcerns?.ToList() ?? BuildDetectedConcerns(forecast),
                    ErrorMessage = llmError == null ? null : "Không thể tạo phần phân tích AI từ mô hình trong lần gọi này. Hệ thống đang trả về tóm tắt tối thiểu từ dữ liệu thời tiết thô."
                }
            };
        }

        private static string BuildFallbackSummary(DisasterType? requestedDisasterType, WeatherForecastResult forecast)
        {
            var peakDay = forecast.Days.OrderByDescending(x => x.PrecipMm).ThenByDescending(x => x.PrecipProbability).FirstOrDefault();
            var target = requestedDisasterType?.ToString() ?? "rủi ro thời tiết";

            return peakDay == null
                ? $"AI chưa phản hồi kịp. Hệ thống đang trả về dữ liệu thời tiết tham khảo để bạn tự xem xu hướng {target.ToLowerInvariant()} trong 14 ngày tới."
                : $"AI chưa phản hồi kịp. Dựa trên forecast thô, thời điểm cần lưu ý nhất hiện quanh ngày {peakDay.Date:dd/MM}, khi lượng mưa dự báo khoảng {peakDay.PrecipMm:0.##} mm.";
        }

        private static string BuildFallbackDetailedAnalysis(WeatherForecastResult forecast)
        {
            var firstWindow = forecast.Days.Take(3).ToList();
            var midWindow = forecast.Days.Skip(3).Take(4).ToList();
            var lateWindow = forecast.Days.Skip(7).Take(7).ToList();

            return $"Trong 1-3 ngày tới, lượng mưa cộng dồn khoảng {firstWindow.Sum(x => x.PrecipMm):0.##} mm. " +
                   $"Giai đoạn 4-7 ngày tiếp theo có tổng mưa khoảng {midWindow.Sum(x => x.PrecipMm):0.##} mm. " +
                   $"Ở nửa sau chu kỳ dự báo, tổng mưa khoảng {lateWindow.Sum(x => x.PrecipMm):0.##} mm. " +
                   "Đây là phần tóm tắt tối thiểu từ dữ liệu forecast, không phải diễn giải hoàn chỉnh từ mô hình AI.";
        }

        private static List<string> BuildFallbackRecommendations(WeatherForecastResult forecast)
        {
            var peakDay = forecast.Days.OrderByDescending(x => x.PrecipMm).ThenByDescending(x => x.PrecipProbability).FirstOrDefault();

            return new List<string>
            {
                "Theo dõi cập nhật forecast hằng ngày, đặc biệt tại các khu vực trũng thấp và điểm ngập quen thuộc.",
                peakDay == null
                    ? "Chuẩn bị phương án điều phối cơ bản theo dữ liệu forecast hiện có."
                    : $"Ưu tiên rà soát năng lực ứng phó trước ngày {peakDay.Date:dd/MM}, là thời điểm forecast có mưa đáng chú ý nhất.",
                "Kiểm tra trước các tuyến đường tiếp cận, điểm sơ tán tạm và nhu yếu phẩm dự phòng nếu mưa kéo dài hơn dự kiến."
            };
        }

        private static List<string> BuildFallbackScenarios(WeatherForecastResult forecast)
        {
            var peakDay = forecast.Days.OrderByDescending(x => x.PrecipMm).ThenByDescending(x => x.PrecipProbability).FirstOrDefault();
            return peakDay == null
                ? new List<string>()
                : new List<string>
                {
                    $"Nếu mưa tập trung hơn dự kiến quanh ngày {peakDay.Date:dd/MM}, một số khu vực trũng thấp có thể bị ảnh hưởng trước.",
                    "Nếu các đợt mưa nối tiếp nhau trong nhiều ngày, việc tiếp cận hiện trường và điều phối cứu trợ có thể chậm hơn bình thường."
                };
        }

        private static List<string> BuildDetectedConcerns(WeatherForecastResult forecast)
        {
            var concerns = new List<string>();
            if (forecast.Days.Sum(x => x.PrecipMm) >= 50) concerns.Add("Mưa tích lũy nhiều ngày");
            if (forecast.Days.Any(x => x.PrecipMm >= 20)) concerns.Add("Có ngày mưa nổi bật");
            if (forecast.Days.Count(x => x.PrecipProbability >= 70) >= 3) concerns.Add("Xác suất mưa cao lặp lại");
            if (forecast.Days.Any(x => x.WindGustKph >= 35)) concerns.Add("Có gió giật cần lưu ý");
            return concerns;
        }

        private static void ValidateRequest(AnalyzeDisasterRiskRequest request)
        {
            if (request.Latitude < -90 || request.Latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Latitude), "Latitude must be between -90 and 90.");
            }

            if (request.Longitude < -180 || request.Longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Longitude), "Longitude must be between -180 and 180.");
            }

            if (!string.IsNullOrWhiteSpace(request.Model) && request.Model.Trim().Length > 200)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Model), "Model must be 200 characters or fewer.");
            }

            if (!string.IsNullOrWhiteSpace(request.LocationName) && request.LocationName.Trim().Length > 200)
            {
                throw new ArgumentOutOfRangeException(nameof(request.LocationName), "LocationName must be 200 characters or fewer.");
            }

            if (!string.IsNullOrWhiteSpace(request.AdditionalContext) && request.AdditionalContext.Trim().Length > 2000)
            {
                throw new ArgumentOutOfRangeException(nameof(request.AdditionalContext), "AdditionalContext must be 2000 characters or fewer.");
            }
        }
    }
}
