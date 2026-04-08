using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Request;
using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Response;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IDisasterAnalysisService
    {
        Task<AnalyzeDisasterRiskResponse> AnalyzeAsync(
            AnalyzeDisasterRiskRequest request,
            CancellationToken cancellationToken = default);
    }
}
