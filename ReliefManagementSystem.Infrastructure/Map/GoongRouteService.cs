using Microsoft.Extensions.Options;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using System.Text.Json;

namespace ReliefManagementSystem.Infrastructure.Map
{
    public class GoongRouteService : IGoongRouteService
    {
        private readonly HttpClient _httpClient;
        private readonly GoongSettings _settings;

        public GoongRouteService(HttpClient httpClient, IOptions<GoongSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }
        }

        public async Task<GoongRouteResult?> GetRouteAsync(
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng,
            string vehicle = "car",
            CancellationToken cancellationToken = default)
        {
            var origin = $"{originLat},{originLng}";
            var destination = $"{destinationLat},{destinationLng}";

            var endpoint = $"/v2/direction?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}&vehicle={Uri.EscapeDataString(vehicle)}&api_key={Uri.EscapeDataString(_settings.ApiKey)}";
            var result = await TryRequestAsync(endpoint, cancellationToken);
            if (result != null)
            {
                return result;
            }

            // Backward compatibility fallback for alternate path casing
            var fallbackEndpoint = $"/Direction?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}&vehicle={Uri.EscapeDataString(vehicle)}&api_key={Uri.EscapeDataString(_settings.ApiKey)}";
            return await TryRequestAsync(fallbackEndpoint, cancellationToken);
        }

        private async Task<GoongRouteResult?> TryRequestAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (!root.TryGetProperty("routes", out var routes) || routes.ValueKind != JsonValueKind.Array || routes.GetArrayLength() == 0)
                {
                    return null;
                }

                var route = routes[0];

                string overviewPolyline = string.Empty;
                if (route.TryGetProperty("overview_polyline", out var overview) && overview.ValueKind == JsonValueKind.Object)
                {
                    if (overview.TryGetProperty("points", out var points))
                    {
                        overviewPolyline = points.GetString() ?? string.Empty;
                    }
                }

                if (string.IsNullOrWhiteSpace(overviewPolyline))
                {
                    return null;
                }

                int? distanceMeters = null;
                if (route.TryGetProperty("legs", out var legs) && legs.ValueKind == JsonValueKind.Array && legs.GetArrayLength() > 0)
                {
                    var leg = legs[0];

                    if (leg.TryGetProperty("distance", out var dist) &&
                        dist.TryGetProperty("value", out var distVal) &&
                        distVal.ValueKind == JsonValueKind.Number)
                    {
                        distanceMeters = distVal.GetInt32();
                    }

                    int? durationFromLeg = null;
                    if (leg.TryGetProperty("duration", out var dur) &&
                        dur.TryGetProperty("value", out var durVal) &&
                        durVal.ValueKind == JsonValueKind.Number)
                    {
                        durationFromLeg = durVal.GetInt32();
                    }

                    // Some Direction responses expose static_duration; prefer realtime duration if available
                    int? staticDuration = null;
                    if (leg.TryGetProperty("static_duration", out var staticDur) &&
                        staticDur.TryGetProperty("value", out var staticDurVal) &&
                        staticDurVal.ValueKind == JsonValueKind.Number)
                    {
                        staticDuration = staticDurVal.GetInt32();
                    }

                    return new GoongRouteResult
                    {
                        OverviewPolyline = overviewPolyline,
                        DistanceMeters = distanceMeters,
                        DurationSeconds = durationFromLeg ?? staticDuration
                    };
                }

                return new GoongRouteResult
                {
                    OverviewPolyline = overviewPolyline,
                    DistanceMeters = distanceMeters,
                    DurationSeconds = null
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }
    }
}
