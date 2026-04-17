using Microsoft.Extensions.Options;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Notification;
using ReliefManagementSystem.Domain.Entities;
using System.Text.Json;
using System.Text;

namespace ReliefManagementSystem.Infrastructure.Security
{
    public class NotificationRealtimePublisher : INotificationRealtimePublisher
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly CentrifugoSettings _centrifugoSettings;

        public NotificationRealtimePublisher(
            HttpClient httpClient,
            IOptions<CentrifugoSettings> centrifugoOptions)
        {
            _httpClient = httpClient;
            _centrifugoSettings = centrifugoOptions.Value;
        }

        public async Task PublishAsync(Notification notification)
        {
            var metadata = ParseMetadata(notification.MetadataJson);
            var payload = new RealtimeNotificationDto
            {
                NotificationId = notification.NotificationId,
                RecipientId = notification.RecipientId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                ReferenceId = notification.ReferenceId,
                ReferenceType = notification.ReferenceType,
                MetadataJson = notification.MetadataJson,
                Metadata = metadata,
                AttachmentCount = metadata.AttachmentCount,
                ThumbnailUrls = metadata.ThumbnailUrls,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };

            var publishRequest = new
            {
                channel = GetNotificationChannel(notification.RecipientId),
                data = payload
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/publish")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(publishRequest, JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("X-API-Key", _centrifugoSettings.HttpApiKey);

            using var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Centrifugo publish failed with status {(int)response.StatusCode}: {responseText}",
                    null,
                    response.StatusCode);
            }

            var apiResponse = DeserializeResponse(responseText);
            if (apiResponse?.Error is not null)
            {
                throw new InvalidOperationException(
                    $"Centrifugo publish error {apiResponse.Error.Code}: {apiResponse.Error.Message}");
            }
        }

        private static string GetNotificationChannel(Guid recipientId)
            => $"notifications_user_{recipientId}";

        private static NotificationMetadataDto ParseMetadata(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return new NotificationMetadataDto();
            }

            try
            {
                return JsonSerializer.Deserialize<NotificationMetadataDto>(metadataJson)
                    ?? new NotificationMetadataDto();
            }
            catch
            {
                return new NotificationMetadataDto();
            }
        }

        private static CentrifugoApiResponse? DeserializeResponse(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<CentrifugoApiResponse>(responseText, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to parse Centrifugo publish response: {responseText}", ex);
            }
        }

        private sealed class CentrifugoApiResponse
        {
            public CentrifugoApiError? Error { get; set; }
        }

        private sealed class CentrifugoApiError
        {
            public int Code { get; set; }
            public string? Message { get; set; }
        }
    }
}
