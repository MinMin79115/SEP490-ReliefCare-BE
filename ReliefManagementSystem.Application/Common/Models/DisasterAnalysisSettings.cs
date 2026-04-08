namespace ReliefManagementSystem.Application.Common.Models
{
    public class DisasterAnalysisSettings
    {
        public int TopRiskCount { get; set; } = 3;
        public bool IncludeEarthquakeInAutoDetect { get; set; } = false;
    }
}
