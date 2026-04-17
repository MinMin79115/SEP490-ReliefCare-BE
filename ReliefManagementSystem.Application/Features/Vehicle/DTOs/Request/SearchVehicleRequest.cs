namespace ReliefManagementSystem.Application.Features.Vehicle.DTOs.Request
{
    public class SearchVehicleRequest
    {
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public Guid? ReliefStationId { get; set; }
    }
}
