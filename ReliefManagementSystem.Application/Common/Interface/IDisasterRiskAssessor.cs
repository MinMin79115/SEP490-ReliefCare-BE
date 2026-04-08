using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IDisasterRiskAssessor
    {
        DisasterRiskAssessment Assess(CurrentWeatherResult weather, DisasterType disasterType, string locationName, string? additionalContext = null);
    }

    public class DisasterRiskAssessment
    {
        public DisasterType DisasterType { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? AdditionalContext { get; set; }
        public CurrentWeatherResult WeatherSnapshot { get; set; } = new();
        public int OverallRiskScore { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public string AssessmentConfidence { get; set; } = "Medium";
        public string? DataLimitationNote { get; set; }
        public List<string> TriggerFactors { get; set; } = new();
        public List<string> PotentialScenarios { get; set; } = new();
        public List<string> TopThreats { get; set; } = new();

        public HeuristicRiskAssessmentDto ToDto()
        {
            return new HeuristicRiskAssessmentDto
            {
                OverallRiskScore = OverallRiskScore,
                RiskLevel = RiskLevel,
                AssessmentConfidence = AssessmentConfidence,
                DataLimitationNote = DataLimitationNote,
                TriggerFactors = TriggerFactors.ToList(),
                PotentialScenarios = PotentialScenarios.ToList(),
                TopThreats = TopThreats.ToList()
            };
        }
    }
}
