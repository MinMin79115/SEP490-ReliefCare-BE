namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ILlmAnalysisService
    {
        Task<LlmDisasterAnalysisResult> AnalyzeRiskAsync(
            DisasterRiskAssessment primaryAssessment,
            IReadOnlyCollection<DisasterRiskAssessment>? rankedAssessments = null,
            string? requestedModel = null,
            CancellationToken cancellationToken = default);
    }

    public class LlmDisasterAnalysisResult
    {
        public string ProviderName { get; set; } = string.Empty;
        public string ModelUsed { get; set; } = string.Empty;
        public string PromptVersion { get; set; } = "disaster-analysis-v1";
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
        public string Summary { get; set; } = string.Empty;
        public string DetailedAnalysis { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
        public List<string> PotentialScenarios { get; set; } = new();
        public string RawResponse { get; set; } = string.Empty;
    }
}
