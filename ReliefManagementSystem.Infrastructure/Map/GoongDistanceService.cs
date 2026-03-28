using Microsoft.Extensions.Options;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using System.Text.Json;

namespace ReliefManagementSystem.Infrastructure.Map
{
    public class GoongDistanceService : IGoongDistanceService
    {
        private readonly HttpClient _httpClient;
        private readonly GoongSettings _settings;

        public GoongDistanceService(HttpClient httpClient, IOptions<GoongSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }
        }

        public async Task<GoongDistanceMatrixResult> GetDistanceMatrixAsync(
            double originLat,
            double originLng,
            IReadOnlyList<(double lat, double lng)> destinations,
            string vehicle = "car",
            CancellationToken cancellationToken = default)
        {
            if (destinations == null || destinations.Count == 0)
            {
                return new GoongDistanceMatrixResult();
            }

            var origins = $"{originLat},{originLng}";
            var destinationParam = string.Join("|", destinations.Select(d => $"{d.lat},{d.lng}"));

            var url = $"/v2/distancematrix?origins={Uri.EscapeDataString(origins)}&destinations={Uri.EscapeDataString(destinationParam)}&vehicle={Uri.EscapeDataString(vehicle)}&api_key={Uri.EscapeDataString(_settings.ApiKey)}";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);

            var result = new GoongDistanceMatrixResult();

            if (!doc.RootElement.TryGetProperty("rows", out var rows) || rows.GetArrayLength() == 0)
                return result;

            var elements = rows[0].GetProperty("elements");
            foreach (var el in elements.EnumerateArray())
            {
                var item = new GoongDistanceElement
                {
                    Status = el.TryGetProperty("status", out var s) ? s.GetString() ?? string.Empty : string.Empty
                };

                if (el.TryGetProperty("distance", out var dist) && dist.TryGetProperty("value", out var dv) && dv.ValueKind == JsonValueKind.Number)
                    item.DistanceMeters = dv.GetInt32();

                if (el.TryGetProperty("duration", out var dur) && dur.TryGetProperty("value", out var tv) && tv.ValueKind == JsonValueKind.Number)
                    item.DurationSeconds = tv.GetInt32();

                result.Elements.Add(item);
            }

            return result;
        }
    }
}
