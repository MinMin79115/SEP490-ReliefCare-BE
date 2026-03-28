namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    public class DistanceMatrixProbeResponse
    {
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public List<DistanceMatrixProbeItem> Items { get; set; } = new();
    }

    public class DistanceMatrixProbeItem
    {
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? DistanceMeters { get; set; }
        public int? DurationSeconds { get; set; }
    }
}
