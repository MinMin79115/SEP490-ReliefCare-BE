namespace ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Request
{
    public class AnalyzeDisasterRiskRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? LocationName { get; set; }
        public string? AdditionalContext { get; set; }
    }
}
