namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IDisasterForecastService
    {
        Task<WeatherForecastResult> GetFloodForecastAsync(
            double latitude,
            double longitude,
            int days = 14,
            CancellationToken cancellationToken = default);
    }

    public class WeatherForecastResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ResolvedAddress { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public double? TimeZoneOffset { get; set; }
        public int RequestedDays { get; set; } = 14;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public double? QueryCost { get; set; }
        public List<ForecastDayResult> Days { get; set; } = new();
    }

    public class ForecastDayResult
    {
        public DateTime Date { get; set; }
        public double TempMaxC { get; set; }
        public double TempMinC { get; set; }
        public double PrecipMm { get; set; }
        public double PrecipProbability { get; set; }
        public double PrecipCover { get; set; }
        public List<string> PrecipTypes { get; set; } = new();
        public double Humidity { get; set; }
        public double CloudCover { get; set; }
        public double Pressure { get; set; }
        public double WindSpeedKph { get; set; }
        public double WindGustKph { get; set; }
        public double VisibilityKm { get; set; }
        public double SevereRisk { get; set; }
        public double Cape { get; set; }
        public double Cin { get; set; }
        public double SnowMm { get; set; }
        public double SnowDepthMm { get; set; }
        public string Conditions { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
