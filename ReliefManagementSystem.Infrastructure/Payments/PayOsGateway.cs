using Microsoft.Extensions.Options;
using ReliefManagementSystem.Application.Common.Exceptions.Donation;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Donation.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ReliefManagementSystem.Infrastructure.Payments
{
    public class PayOsGateway : IPayOsGateway
    {
        private readonly HttpClient _httpClient;
        private readonly PayOsSettings _settings;

        public PayOsGateway(HttpClient httpClient, IOptions<PayOsSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;

            _httpClient.BaseAddress ??= new Uri("https://api-merchant.payos.vn");
            if (!_httpClient.DefaultRequestHeaders.Contains("x-client-id"))
            {
                _httpClient.DefaultRequestHeaders.Add("x-client-id", _settings.ClientId);
            }

            if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            {
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
            }
        }

        public async Task<PayOsCreatePaymentResult> CreatePaymentLinkAsync(
            long orderCode,
            int amount,
            string description,
            string buyerName,
            string? buyerEmail,
            string? buyerPhone,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            var signatureData = $"amount={amount}&cancelUrl={_settings.CancelUrl}&description={description}&orderCode={orderCode}&returnUrl={_settings.ReturnUrl}";
            var signature = ComputeSignature(signatureData, _settings.ChecksumKey);

            var payload = new
            {
                orderCode,
                amount,
                description,
                buyerName,
                buyerEmail,
                buyerPhone,
                cancelUrl = _settings.CancelUrl,
                returnUrl = _settings.ReturnUrl,
                expiredAt = new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds(),
                signature
            };

            using var response = await _httpClient.PostAsJsonAsync("/v2/payment-requests", payload, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            EnsureSuccess(response, body);

            var parsed = JsonSerializer.Deserialize<PayOsResponse<PayOsCreateData>>(body, JsonOptions());
            if (parsed?.Data is null)
            {
                throw new PayOsIntegrationException("PayOS không trả về dữ liệu tạo link thanh toán.");
            }

            return new PayOsCreatePaymentResult
            {
                OrderCode = parsed.Data.OrderCode,
                Amount = parsed.Data.Amount,
                PaymentLinkId = parsed.Data.PaymentLinkId,
                CheckoutUrl = parsed.Data.CheckoutUrl ?? string.Empty,
                Status = parsed.Data.Status ?? "PENDING"
            };
        }

        public async Task<PayOsPaymentInfoResult> GetPaymentLinkInfoAsync(string idOrOrderCode, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"/v2/payment-requests/{idOrOrderCode}", cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            EnsureSuccess(response, body);

            var parsed = JsonSerializer.Deserialize<PayOsResponse<PayOsInfoData>>(body, JsonOptions());
            if (parsed?.Data is null)
            {
                throw new PayOsIntegrationException("Không lấy được thông tin payment link từ PayOS.");
            }

            return new PayOsPaymentInfoResult
            {
                PaymentLinkId = parsed.Data.Id,
                OrderCode = parsed.Data.OrderCode,
                Amount = parsed.Data.Amount,
                AmountPaid = parsed.Data.AmountPaid,
                AmountRemaining = parsed.Data.AmountRemaining,
                Status = parsed.Data.Status ?? "PENDING"
            };
        }

        public async Task<PayOsPaymentInfoResult> CancelPaymentLinkAsync(string idOrOrderCode, string? reason, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                $"/v2/payment-requests/{idOrOrderCode}/cancel",
                new { cancellationReason = reason },
                cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            EnsureSuccess(response, body);

            var parsed = JsonSerializer.Deserialize<PayOsResponse<PayOsInfoData>>(body, JsonOptions());
            if (parsed?.Data is null)
            {
                throw new PayOsIntegrationException("Không thể huỷ payment link trên PayOS.");
            }

            return new PayOsPaymentInfoResult
            {
                PaymentLinkId = parsed.Data.Id,
                OrderCode = parsed.Data.OrderCode,
                Amount = parsed.Data.Amount,
                AmountPaid = parsed.Data.AmountPaid,
                AmountRemaining = parsed.Data.AmountRemaining,
                Status = parsed.Data.Status ?? "CANCELLED"
            };
        }

        public bool VerifyWebhook(PayOsWebhookRequest request)
        {
            if (request?.Data is null || string.IsNullOrWhiteSpace(request.Signature))
            {
                return false;
            }

            var dataElement = JsonSerializer.SerializeToElement(request.Data, JsonOptions());
            var map = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (var prop in dataElement.EnumerateObject())
            {
                if (prop.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                {
                    continue;
                }

                var value = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString()
                    : prop.Value.GetRawText();

                if (value is not null)
                {
                    map[prop.Name] = value;
                }
            }

            var canonical = string.Join("&", map.Select(kv => $"{kv.Key}={kv.Value}"));
            var expected = ComputeSignature(canonical, _settings.ChecksumKey);

            return string.Equals(expected, request.Signature, StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureSuccess(HttpResponseMessage response, string body)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new PayOsIntegrationException($"PayOS request failed ({(int)response.StatusCode}): {body}");
            }

            var baseResponse = JsonSerializer.Deserialize<PayOsBaseResponse>(body, JsonOptions());
            if (baseResponse is null || !string.Equals(baseResponse.Code, "00", StringComparison.OrdinalIgnoreCase))
            {
                throw new PayOsIntegrationException($"PayOS business error: {baseResponse?.Desc ?? "Unknown"}");
            }
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private static string ComputeSignature(string data, string checksumKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private class PayOsBaseResponse
        {
            public string? Code { get; set; }
            public string? Desc { get; set; }
        }

        private class PayOsResponse<T> : PayOsBaseResponse
        {
            public T? Data { get; set; }
        }

        private class PayOsCreateData
        {
            public int Amount { get; set; }
            public long OrderCode { get; set; }
            public string? PaymentLinkId { get; set; }
            public string? Status { get; set; }
            public string? CheckoutUrl { get; set; }
        }

        private class PayOsInfoData
        {
            public string? Id { get; set; }
            public long OrderCode { get; set; }
            public int Amount { get; set; }
            public int AmountPaid { get; set; }
            public int AmountRemaining { get; set; }
            public string? Status { get; set; }
        }
    }
}
