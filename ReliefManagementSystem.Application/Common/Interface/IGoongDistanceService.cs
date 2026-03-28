namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IGoongDistanceService
    {
        Task<GoongDistanceMatrixResult> GetDistanceMatrixAsync(
            double originLat,
            double originLng,
            IReadOnlyList<(double lat, double lng)> destinations,
            string vehicle = "car",
            CancellationToken cancellationToken = default);
    }

    public class GoongDistanceMatrixResult
    {
        public List<GoongDistanceElement> Elements { get; set; } = new();
    }

    public class GoongDistanceElement
    {
        public string Status { get; set; } = string.Empty;
        public int? DistanceMeters { get; set; }
        public int? DurationSeconds { get; set; }
    }
}
