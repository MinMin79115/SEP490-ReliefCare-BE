using ReliefManagementSystem.Application.Common.Interface;
using System.Text.Json.Nodes;

namespace ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Response
{
    public class AnalyzeDisasterRiskResponse
    {
        public Guid AnalysisLogId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string AnalysisMode { get; set; } = "AutoDetect";
        public string? RequestedDisasterType { get; set; }
        public WeatherSnapshotDto Weather { get; set; } = new();
        public FloodForecastDto Forecast { get; set; } = new();
        public AiDisasterNarrativeDto Ai { get; set; } = new();
    }

    public class WeatherSnapshotDto
    {
        public DateTime ObservedAt { get; set; }
        public string Condition { get; set; } = string.Empty;
        public double TemperatureC { get; set; }
        public double WindKph { get; set; }
        public double PrecipMm { get; set; }
        public double VisibilityKm { get; set; }
        public int Humidity { get; set; }
        public int BaseWeatherRiskScore { get; set; }
        public string BaseWeatherRiskLevel { get; set; } = string.Empty;

        public static WeatherSnapshotDto From(CurrentWeatherResult weather)
        {
            return new WeatherSnapshotDto
            {
                ObservedAt = weather.ObservedAt,
                Condition = weather.Condition,
                TemperatureC = weather.TemperatureC,
                WindKph = weather.WindKph,
                PrecipMm = weather.PrecipMm,
                VisibilityKm = weather.VisibilityKm,
                Humidity = weather.Humidity,
                BaseWeatherRiskScore = weather.WeatherRiskScore,
                BaseWeatherRiskLevel = weather.WeatherRiskLevel
            };
        }
    }

    public class FloodForecastDto
    {
        public string ResolvedAddress { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public int RequestedDays { get; set; }
        public DateTime GeneratedAt { get; set; }
        public double? QueryCost { get; set; }
        public double TotalPrecipMm { get; set; }
        public double MaxDailyPrecipMm { get; set; }
        public DateTime? PeakRainDate { get; set; }
        public int ConsecutiveRainyDaysPeak { get; set; }
        public List<ForecastDayDto> Days { get; set; } = new();

        public static FloodForecastDto From(WeatherForecastResult forecast)
        {
            var orderedDays = forecast.Days.OrderBy(x => x.Date).ToList();
            var peakDay = orderedDays
                .OrderByDescending(x => x.PrecipMm)
                .ThenByDescending(x => x.PrecipProbability)
                .FirstOrDefault();

            var rainyStreak = 0;
            var maxRainyStreak = 0;

            foreach (var day in orderedDays)
            {
                if (day.PrecipMm >= 5 || day.PrecipProbability >= 60)
                {
                    rainyStreak++;
                    maxRainyStreak = Math.Max(maxRainyStreak, rainyStreak);
                }
                else
                {
                    rainyStreak = 0;
                }
            }

            return new FloodForecastDto
            {
                ResolvedAddress = forecast.ResolvedAddress,
                TimeZone = forecast.TimeZone,
                RequestedDays = forecast.RequestedDays,
                GeneratedAt = forecast.GeneratedAt,
                QueryCost = forecast.QueryCost,
                TotalPrecipMm = orderedDays.Sum(x => x.PrecipMm),
                MaxDailyPrecipMm = peakDay?.PrecipMm ?? 0,
                PeakRainDate = peakDay?.Date,
                ConsecutiveRainyDaysPeak = maxRainyStreak,
                Days = orderedDays.Select(x => new ForecastDayDto
                {
                    Date = x.Date,
                    TempMaxC = x.TempMaxC,
                    TempMinC = x.TempMinC,
                    PrecipMm = x.PrecipMm,
                    PrecipProbability = x.PrecipProbability,
                    PrecipCover = x.PrecipCover,
                    Humidity = x.Humidity,
                    Pressure = x.Pressure,
                    WindSpeedKph = x.WindSpeedKph,
                    WindGustKph = x.WindGustKph,
                    VisibilityKm = x.VisibilityKm,
                    SevereRisk = x.SevereRisk,
                    Conditions = x.Conditions,
                    Description = x.Description,
                    PrecipTypes = x.PrecipTypes.ToList()
                }).ToList()
            };
        }
    }

    public class ForecastDayDto
    {
        public DateTime Date { get; set; }
        public double TempMaxC { get; set; }
        public double TempMinC { get; set; }
        public double PrecipMm { get; set; }
        public double PrecipProbability { get; set; }
        public double PrecipCover { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public double WindSpeedKph { get; set; }
        public double WindGustKph { get; set; }
        public double VisibilityKm { get; set; }
        public double SevereRisk { get; set; }
        public string Conditions { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> PrecipTypes { get; set; } = new();
    }

    public class AiDisasterNarrativeDto
    {
        public bool Succeeded { get; set; }
        public string? Provider { get; set; }
        public string? Model { get; set; }
        public string? PromptVersion { get; set; }
        public DateTime? AnalyzedAt { get; set; }
        public string? RequestedRiskType { get; set; }
        public string? Summary { get; set; }
        public string? DetailedAnalysis { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public List<string> PotentialScenarios { get; set; } = new();
        public List<string> DetectedConcerns { get; set; } = new();
        public JsonNode? LlmResponse { get; set; }
        public JsonNode? TriggerFactors { get; set; }
        public JsonNode? TopThreats { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class NearestDisasterAnalysisResponse
    {
        public Guid AnalysisLogId { get; set; }
        public double RequestedLatitude { get; set; }
        public double RequestedLongitude { get; set; }
        public double MatchedLatitude { get; set; }
        public double MatchedLongitude { get; set; }
        public double DistanceKm { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string DisasterType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string HeuristicRiskLevel { get; set; } = string.Empty;
        public int HeuristicRiskScore { get; set; }
        public string AssessmentConfidence { get; set; } = string.Empty;
        public string? DataLimitationNote { get; set; }
        public string? LlmProvider { get; set; }
        public string? LlmModel { get; set; }
        public string? PromptVersion { get; set; }
        public string? ErrorMessage { get; set; }
        public JsonNode? WeatherSnapshot { get; set; }
        public JsonNode? LlmResponse { get; set; }
        public JsonNode? PotentialScenarios { get; set; }
        public JsonNode? TriggerFactors { get; set; }
        public JsonNode? TopThreats { get; set; }
    }
}
