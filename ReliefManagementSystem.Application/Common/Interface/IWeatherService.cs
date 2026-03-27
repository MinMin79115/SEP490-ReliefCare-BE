namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IWeatherService
    {
        Task<CurrentWeatherResult> GetCurrentWeatherAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default);
    }

    public class CurrentWeatherResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime ObservedAt { get; set; }

        public string Condition { get; set; } = string.Empty;
        public double TemperatureC { get; set; }
        public double WindKph { get; set; }
        public double PrecipMm { get; set; }
        public double VisibilityKm { get; set; }
        public int Humidity { get; set; }

        public int WeatherRiskScore { get; set; }
        public string WeatherRiskLevel { get; set; } = "Low";
    }
}
