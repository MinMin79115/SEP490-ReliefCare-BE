using Microsoft.Extensions.Options;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using System.Text.Json;

namespace ReliefManagementSystem.Infrastructure.Weather
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherApiSettings _settings;

        public WeatherService(HttpClient httpClient, IOptions<WeatherApiSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }
        }

        public async Task<CurrentWeatherResult> GetCurrentWeatherAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default)
        {
            var q = $"{latitude},{longitude}";
            var normalizedBaseUrl = (_settings.BaseUrl ?? "https://api.weatherapi.com/v1").TrimEnd('/');
            if (!normalizedBaseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            {
                normalizedBaseUrl += "/v1";
            }

            var url = $"{normalizedBaseUrl}/current.json?key={Uri.EscapeDataString(_settings.ApiKey)}&q={Uri.EscapeDataString(q)}&aqi=no";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);

            var current = doc.RootElement.GetProperty("current");
            var observedAt = current.TryGetProperty("last_updated", out var lu)
                && DateTime.TryParse(lu.GetString(), out var parsed)
                ? parsed
                : DateTime.UtcNow;

            observedAt = observedAt.Kind switch
            {
                DateTimeKind.Utc => observedAt,
                DateTimeKind.Local => observedAt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(observedAt, DateTimeKind.Utc)
            };

            var condition = current.GetProperty("condition").GetProperty("text").GetString() ?? string.Empty;
            var tempC = current.TryGetProperty("temp_c", out var tc) ? tc.GetDouble() : 0;
            var windKph = current.TryGetProperty("wind_kph", out var wk) ? wk.GetDouble() : 0;
            var precipMm = current.TryGetProperty("precip_mm", out var pm) ? pm.GetDouble() : 0;
            var visKm = current.TryGetProperty("vis_km", out var vk) ? vk.GetDouble() : 0;
            var humidity = current.TryGetProperty("humidity", out var hm) ? hm.GetInt32() : 0;

            var score = CalculateWeatherRiskScore(tempC, windKph, precipMm, visKm, humidity, condition);

            return new CurrentWeatherResult
            {
                Latitude = latitude,
                Longitude = longitude,
                ObservedAt = observedAt,
                Condition = condition,
                TemperatureC = tempC,
                WindKph = windKph,
                PrecipMm = precipMm,
                VisibilityKm = visKm,
                Humidity = humidity,
                WeatherRiskScore = score,
                WeatherRiskLevel = ToRiskLevel(score)
            };
        }

        private static int CalculateWeatherRiskScore(
            double tempC,
            double windKph,
            double precipMm,
            double visKm,
            int humidity,
            string condition)
        {
            var score = 0;

            if (windKph >= 40) score += 30;
            else if (windKph >= 25) score += 18;
            else if (windKph >= 15) score += 10;

            if (precipMm >= 20) score += 30;
            else if (precipMm >= 10) score += 20;
            else if (precipMm >= 2) score += 10;

            if (visKm <= 1) score += 20;
            else if (visKm <= 3) score += 12;
            else if (visKm <= 6) score += 6;

            if (tempC >= 38 || tempC <= 8) score += 10;
            if (humidity >= 90) score += 5;

            var c = condition.ToLowerInvariant();
            if (c.Contains("storm") || c.Contains("thunder") || c.Contains("squall")) score += 20;
            else if (c.Contains("heavy rain") || c.Contains("rain") || c.Contains("fog")) score += 10;

            return Math.Clamp(score, 0, 100);
        }

        private static string ToRiskLevel(int score)
        {
            if (score >= 70) return "High";
            if (score >= 40) return "Medium";
            return "Low";
        }
    }
}
