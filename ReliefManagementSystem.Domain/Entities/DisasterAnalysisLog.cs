using ReliefManagementSystem.Domain.Entities.Common;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class DisasterAnalysisLog : AuditableEntity
    {
        public Guid DisasterAnalysisLogId { get; set; }
        public Guid? RescueRequestId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DisasterType DisasterType { get; set; }
        public string? RequestedModel { get; set; }
        public string? AdditionalContext { get; set; }
        public string WeatherSnapshotJson { get; set; } = string.Empty;
        public int HeuristicRiskScore { get; set; }
        public string HeuristicRiskLevel { get; set; } = string.Empty;
        public string AssessmentConfidence { get; set; } = string.Empty;
        public string TriggerFactorsJson { get; set; } = string.Empty;
        public string PotentialScenariosJson { get; set; } = string.Empty;
        public string TopThreatsJson { get; set; } = string.Empty;
        public string? DataLimitationNote { get; set; }
        public string? LlmProvider { get; set; }
        public string? LlmModel { get; set; }
        public string? PromptVersion { get; set; }
        public string? LlmResponseJson { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
