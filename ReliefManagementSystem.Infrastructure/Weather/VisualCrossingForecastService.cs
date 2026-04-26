using Microsoft.Extensions.Options;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using System.Globalization;
using System.Text.Json;

namespace ReliefManagementSystem.Infrastructure.Weather
{
    public class VisualCrossingForecastService : IDisasterForecastService
    {
        private readonly HttpClient _httpClient;
        private readonly VisualCrossingSettings _settings;

        public VisualCrossingForecastService(HttpClient httpClient, IOptions<VisualCrossingSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;

            if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
            }
        }

        public async Task<WeatherForecastResult> GetFloodForecastAsync(
            double latitude,
            double longitude,
            int days = 14,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new InvalidOperationException("VisualCrossing:ApiKey is missing.");
            }

            var requestedDays = Math.Clamp(days <= 0 ? _settings.ForecastDays : days, 1, 14);
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var endDate = startDate.AddDays(requestedDays - 1);
            var location = $"{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";

            var query = new List<string>
            {
                $"key={Uri.EscapeDataString(_settings.ApiKey)}",
                $"unitGroup={Uri.EscapeDataString(_settings.UnitGroup)}",
                "contentType=json",
                $"include={Uri.EscapeDataString(_settings.Include)}",
                $"elements={Uri.EscapeDataString(_settings.Elements)}"
            };

            if (_settings.ExcludeNullValues)
            {
                query.Add("options=nonulls");
            }

            var requestPath = $"{location}/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}?{string.Join("&", query)}";

            using var response = await _httpClient.GetAsync(requestPath, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var result = new WeatherForecastResult
            {
                Latitude = GetDouble(root, "latitude", latitude),
                Longitude = GetDouble(root, "longitude", longitude),
                ResolvedAddress = GetString(root, "resolvedAddress"),
                TimeZone = GetString(root, "timezone"),
                TimeZoneOffset = root.TryGetProperty("tzoffset", out var tzOffset) && tzOffset.ValueKind == JsonValueKind.Number
                    ? tzOffset.GetDouble()
                    : null,
                RequestedDays = requestedDays,
                GeneratedAt = DateTime.UtcNow,
                QueryCost = root.TryGetProperty("queryCost", out var qc) && qc.ValueKind == JsonValueKind.Number ? qc.GetDouble() : null,
                Days = root.TryGetProperty("days", out var daysEl) && daysEl.ValueKind == JsonValueKind.Array
                    ? daysEl.EnumerateArray().Select(MapDay).Take(requestedDays).ToList()
                    : new List<ForecastDayResult>()
            };

            if (result.Days.Count == 0)
            {
                throw new InvalidOperationException("Visual Crossing returned no forecast days.");
            }

            return result;
        }

        private static ForecastDayResult MapDay(JsonElement day)
        {
            var parsedDate = day.TryGetProperty("datetime", out var dateElement)
                && DateTime.TryParse(dateElement.GetString(), out var date)
                    ? date
                    : DateTime.UtcNow.Date;

            return new ForecastDayResult
            {
                Date = DateTime.SpecifyKind(parsedDate, DateTimeKind.Unspecified),
                TempMaxC = GetDouble(day, "tempmax"),
                TempMinC = GetDouble(day, "tempmin"),
                PrecipMm = GetDouble(day, "precip"),
                PrecipProbability = GetDouble(day, "precipprob"),
                PrecipCover = GetDouble(day, "precipcover"),
                PrecipTypes = GetStringArray(day, "preciptype"),
                Humidity = GetDouble(day, "humidity"),
                CloudCover = GetDouble(day, "cloudcover"),
                Pressure = GetDouble(day, "pressure"),
                WindSpeedKph = GetDouble(day, "windspeed"),
                WindGustKph = GetDouble(day, "windgust"),
                VisibilityKm = GetDouble(day, "visibility"),
                SevereRisk = GetDouble(day, "severerisk"),
                Cape = GetDouble(day, "cape"),
                Cin = GetDouble(day, "cin"),
                SnowMm = GetDouble(day, "snow"),
                SnowDepthMm = GetDouble(day, "snowdepth"),
                Conditions = GetString(day, "conditions"),
                Description = GetString(day, "description"),
                Source = GetString(day, "source")
            };
        }

        private static string GetString(JsonElement element, string property)
            => element.TryGetProperty(property, out var value) ? value.GetString() ?? string.Empty : string.Empty;

        private static double GetDouble(JsonElement element, string property, double fallback = 0)
            => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : fallback;

        private static List<string> GetStringArray(JsonElement element, string property)
        {
            if (!element.TryGetProperty(property, out var value))
            {
                return new List<string>();
            }

            if (value.ValueKind == JsonValueKind.Array)
            {
                return value.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .ToList();
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                var raw = value.GetString();
                return string.IsNullOrWhiteSpace(raw)
                    ? new List<string>()
                    : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }

            return new List<string>();
        }
    }
}
