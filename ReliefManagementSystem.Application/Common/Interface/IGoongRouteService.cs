using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IGoongRouteService
    {
        Task<GoongRouteResult?> GetRouteAsync(
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng,
            string vehicle = "car",
            CancellationToken cancellationToken = default);
    }

    public class GoongRouteResult
    {
        public string OverviewPolyline { get; set; } = string.Empty;
        public int? DistanceMeters { get; set; }
        public int? DurationSeconds { get; set; }
    }
}
