using ReliefManagementSystem.Application.Common.Interface;

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
        public string PrimaryDisasterType { get; set; } = string.Empty;
        public WeatherSnapshotDto Weather { get; set; } = new();
        public List<DisasterRiskRankingDto> RiskRanking { get; set; } = new();
        public HeuristicRiskAssessmentDto Heuristic { get; set; } = new();
        public AiDisasterNarrativeDto Ai { get; set; } = new();
    }

    public class DisasterRiskRankingDto
    {
        public string DisasterType { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string AssessmentConfidence { get; set; } = string.Empty;
        public List<string> TriggerFactors { get; set; } = new();
        public List<string> TopThreats { get; set; } = new();
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

    public class HeuristicRiskAssessmentDto
    {
        public int OverallRiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string AssessmentConfidence { get; set; } = string.Empty;
        public string? DataLimitationNote { get; set; }
        public List<string> TriggerFactors { get; set; } = new();
        public List<string> PotentialScenarios { get; set; } = new();
        public List<string> TopThreats { get; set; } = new();
    }

    public class AiDisasterNarrativeDto
    {
        public bool Succeeded { get; set; }
        public string? Provider { get; set; }
        public string? Model { get; set; }
        public string? PromptVersion { get; set; }
        public DateTime? AnalyzedAt { get; set; }
        public string? PrimaryRiskType { get; set; }
        public string? Summary { get; set; }
        public string? DetailedAnalysis { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public List<string> PotentialScenarios { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
