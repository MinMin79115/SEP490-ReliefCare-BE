namespace ReliefManagementSystem.Application.Common.Models
{
    public class WeatherApiSettings
    {
        public string BaseUrl { get; set; } = "https://api.weatherapi.com/v1";
        public string ApiKey { get; set; } = string.Empty;
    }
}
