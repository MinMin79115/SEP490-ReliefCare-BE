namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ILlmAnalysisService
    {
        Task<LlmDisasterAnalysisResult> AnalyzeRiskAsync(
            CurrentWeatherResult weather,
            WeatherForecastResult forecast,
            string locationName,
            string? requestedDisasterType = null,
            string? additionalContext = null,
            string? requestedModel = null,
            CancellationToken cancellationToken = default);
    }

    public class LlmDisasterAnalysisResult
    {
        public string ProviderName { get; set; } = string.Empty;
        public string ModelUsed { get; set; } = string.Empty;
        public string PromptVersion { get; set; } = "disaster-analysis-v3-pure-ai";
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
        public string Summary { get; set; } = string.Empty;
        public string DetailedAnalysis { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
        public List<string> PotentialScenarios { get; set; } = new();
        public List<string> DetectedConcerns { get; set; } = new();
        public string RawResponse { get; set; } = string.Empty;
    }
}
