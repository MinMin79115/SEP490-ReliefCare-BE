using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Request;
using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ReliefManagementSystem.Application.Services
{
    public class DisasterAnalysisService : IDisasterAnalysisService
    {
        private readonly IWeatherService _weatherService;
        private readonly IDisasterRiskAssessor _disasterRiskAssessor;
        private readonly ILlmAnalysisService _llmAnalysisService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DisasterAnalysisSettings _settings;

        public DisasterAnalysisService(
            IWeatherService weatherService,
            IDisasterRiskAssessor disasterRiskAssessor,
            ILlmAnalysisService llmAnalysisService,
            IUnitOfWork unitOfWork,
            IOptions<DisasterAnalysisSettings> options)
        {
            _weatherService = weatherService;
            _disasterRiskAssessor = disasterRiskAssessor;
            _llmAnalysisService = llmAnalysisService;
            _unitOfWork = unitOfWork;
            _settings = options.Value;
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

            var supportedDisasterTypes = GetSupportedDisasterTypes(_settings.IncludeEarthquakeInAutoDetect).ToList();
            var allAssessments = supportedDisasterTypes
                .Select(disasterType => _disasterRiskAssessor.Assess(
                    weather,
                    disasterType,
                    locationName,
                    request.AdditionalContext))
                .OrderByDescending(x => x.OverallRiskScore)
                .ThenByDescending(x => x.TriggerFactors.Count)
                .ToList();

            var requestedDisasterType = request.DisasterType;
            var primaryAssessment = requestedDisasterType.HasValue
                ? allAssessments.FirstOrDefault(x => x.DisasterType == requestedDisasterType.Value)
                : allAssessments.FirstOrDefault();

            if (primaryAssessment == null)
            {
                throw new InvalidOperationException("No disaster assessment could be generated from the current weather data.");
            }

            var riskRanking = allAssessments
                .Take(Math.Clamp(_settings.TopRiskCount, 1, Math.Max(1, allAssessments.Count)))
                .ToList();

            var analysisMode = requestedDisasterType.HasValue ? "Focused" : "AutoDetect";

            LlmDisasterAnalysisResult? llmResult = null;
            string? llmError = null;

            try
            {
                llmResult = await _llmAnalysisService.AnalyzeRiskAsync(
                    primaryAssessment,
                    riskRanking,
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
                DisasterType = primaryAssessment.DisasterType,
                RequestedModel = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model.Trim(),
                AdditionalContext = request.AdditionalContext,
                WeatherSnapshotJson = JsonSerializer.Serialize(weather),
                HeuristicRiskScore = primaryAssessment.OverallRiskScore,
                HeuristicRiskLevel = primaryAssessment.RiskLevel,
                AssessmentConfidence = primaryAssessment.AssessmentConfidence,
                DataLimitationNote = primaryAssessment.DataLimitationNote,
                TriggerFactorsJson = JsonSerializer.Serialize(primaryAssessment.TriggerFactors),
                PotentialScenariosJson = JsonSerializer.Serialize(primaryAssessment.PotentialScenarios),
                TopThreatsJson = JsonSerializer.Serialize(primaryAssessment.TopThreats),
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
                PrimaryDisasterType = primaryAssessment.DisasterType.ToString(),
                Weather = WeatherSnapshotDto.From(weather),
                RiskRanking = riskRanking.Select(MapRiskRanking).ToList(),
                Heuristic = primaryAssessment.ToDto(),
                Ai = new AiDisasterNarrativeDto
                {
                    Succeeded = llmResult != null,
                    Provider = llmResult?.ProviderName,
                    Model = llmResult?.ModelUsed,
                    PromptVersion = llmResult?.PromptVersion,
                    AnalyzedAt = llmResult?.AnalyzedAt,
                    PrimaryRiskType = primaryAssessment.DisasterType.ToString(),
                    Summary = llmResult?.Summary,
                    DetailedAnalysis = llmResult?.DetailedAnalysis,
                    Recommendations = llmResult?.Recommendations?.ToList() ?? new List<string>(),
                    PotentialScenarios = llmResult?.PotentialScenarios?.ToList() ?? primaryAssessment.PotentialScenarios.ToList(),
                    ErrorMessage = llmError
                }
            };
        }

        private static DisasterRiskRankingDto MapRiskRanking(DisasterRiskAssessment assessment)
        {
            return new DisasterRiskRankingDto
            {
                DisasterType = assessment.DisasterType.ToString(),
                RiskScore = assessment.OverallRiskScore,
                RiskLevel = assessment.RiskLevel,
                AssessmentConfidence = assessment.AssessmentConfidence,
                TriggerFactors = assessment.TriggerFactors.ToList(),
                TopThreats = assessment.TopThreats.ToList()
            };
        }

        private static IEnumerable<DisasterType> GetSupportedDisasterTypes(bool includeEarthquakeInAutoDetect)
        {
            yield return DisasterType.Flood;
            yield return DisasterType.Storm;
            yield return DisasterType.Landslide;
            yield return DisasterType.Fire;

            if (includeEarthquakeInAutoDetect)
            {
                yield return DisasterType.Earthquake;
            }
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
