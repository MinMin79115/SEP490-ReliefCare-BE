namespace ReliefManagementSystem.Application.Common.Models
{
    public class VisualCrossingSettings
    {
        public string BaseUrl { get; set; } = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline";
        public string ApiKey { get; set; } = string.Empty;
        public string UnitGroup { get; set; } = "metric";
        public int ForecastDays { get; set; } = 14;
        public string Include { get; set; } = "days";
        public string Elements { get; set; } = "datetime,tempmax,tempmin,precip,precipprob,precipcover,humidity,cloudcover,pressure,windspeed,windgust,visibility,severerisk,cape,cin,snow,snowdepth";
        public bool ExcludeNullValues { get; set; } = true;
    }
}
