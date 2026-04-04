namespace ReliefManagementSystem.Application.Features.VehicleType.DTOs.Request
{
    public class SearchVehicleTypeRequest
    {
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
    }
}
