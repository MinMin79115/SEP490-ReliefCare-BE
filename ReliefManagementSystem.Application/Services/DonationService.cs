using ReliefManagementSystem.Application.Common.Exceptions.Donation;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Donation.DTOs.Request;
using ReliefManagementSystem.Application.Features.Donation.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ReliefManagementSystem.Application.Services
{
    public class DonationService : IDonationService
    {
        private const string PayOsProvider = "PayOS";

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPayOsGateway _payOsGateway;
        private readonly ICampaignService _campaignService;

        public DonationService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IPayOsGateway payOsGateway,
            ICampaignService campaignService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _payOsGateway = payOsGateway;
            _campaignService = campaignService;
        }

        public async Task<CreateDonationCheckoutResponse> CreateCheckoutAsync(CreateDonationCheckoutRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(request.CampaignId, cancellationToken);
            if (campaign is null)
            {
                throw new DonationCampaignNotFoundException(request.CampaignId);
            }

            if (campaign.Type != CampaignType.Fundraising)
            {
                throw new DonationInvalidStateException("Chỉ campaign Fundraising mới có thể nhận tiền donation.");
            }

            if (campaign.Status != CampaignStatus.Active)
            {
                throw new DonationInvalidStateException("Campaign không ở trạng thái Active để nhận donation.");
            }

            if (campaign.StartDate > DateTime.UtcNow || campaign.EndDate < DateTime.UtcNow)
            {
                throw new DonationInvalidStateException("Campaign hiện không nằm trong thời gian cho phép nhận donation.");
            }

            if (!campaign.ResourceGoals.Any(g => g.ResourceType == CampaignResourceType.Money))
            {
                throw new DonationInvalidStateException("Campaign không có mục tiêu Money để nhận donation.");
            }

            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(15);
            var orderCode = BuildOrderCode(now);

            var amountInt = Convert.ToInt32(Math.Round(request.Amount, MidpointRounding.AwayFromZero));
            if (amountInt <= 0)
            {
                throw new DonationInvalidStateException("Số tiền donation không hợp lệ.");
            }

            var createResult = await _payOsGateway.CreatePaymentLinkAsync(
                orderCode,
                amountInt,
                BuildDescription(orderCode),
                request.DonorName,
                null,
                null,
                expiresAt,
                cancellationToken);

            var donation = new Domain.Entities.Donation
            {
                DonationId = Guid.NewGuid(),
                CampaignId = request.CampaignId,
                DonorUserId = _currentUserService.UserId,
                DonorName = request.DonorName.Trim(),
                Amount = request.Amount,
                Message = request.Message,
                DonatedAt = now,
                Status = DonationStatus.Pending,
                TransactionRef = createResult.PaymentLinkId,
                GatewayResponse = JsonSerializer.Serialize(createResult),
                PayOsOrderCode = orderCode,
                PayOsPaymentLinkId = createResult.PaymentLinkId,
                CheckoutUrl = createResult.CheckoutUrl,
                ExpiresAt = expiresAt
            };

            await _unitOfWork.Donations.AddAsync(donation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateDonationCheckoutResponse
            {
                DonationId = donation.DonationId,
                OrderCode = donation.PayOsOrderCode ?? 0,
                PaymentLinkId = donation.PayOsPaymentLinkId,
                CheckoutUrl = donation.CheckoutUrl ?? string.Empty,
                ExpiresAt = donation.ExpiresAt,
                Status = donation.Status
            };
        }

        public async Task<DonationStatusResponse> GetStatusAsync(Guid donationId, CancellationToken cancellationToken = default)
        {
            var donation = await _unitOfWork.Donations.GetByIdAsync(donationId);
            if (donation is null)
            {
                throw new DonationNotFoundException(donationId);
            }

            if (donation.Status == DonationStatus.Pending && donation.ExpiresAt <= DateTime.UtcNow)
            {
                donation.Status = DonationStatus.Expired;
                donation.ProcessedAt = DateTime.UtcNow;
                await _unitOfWork.Donations.UpdateAsync(donation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return MapStatus(donation);
        }

        public async Task HandlePayOsWebhookAsync(PayOsWebhookRequest request, CancellationToken cancellationToken = default)
        {
            var isValid = _payOsGateway.VerifyWebhook(request);
            if (!isValid)
            {
                throw new PayOsWebhookSignatureInvalidException();
            }

            var donation = await _unitOfWork.Donations.GetByPayOsOrderCodeAsync(request.Data.OrderCode, cancellationToken);
            if (donation is null)
            {
                return;
            }

            var isDuplicate = await _unitOfWork.PaymentTransactions
                .ExistsByProviderAndReferenceAsync(PayOsProvider, request.Data.Reference, cancellationToken);
            if (isDuplicate)
            {
                return;
            }

            var transaction = new PaymentTransaction
            {
                PaymentTransactionId = Guid.NewGuid(),
                DonationId = donation.DonationId,
                UserId = donation.DonorUserId,
                Provider = PayOsProvider,
                OrderCode = request.Data.OrderCode,
                PaymentLinkId = request.Data.PaymentLinkId,
                Reference = request.Data.Reference,
                EventCode = request.Data.Code,
                EventDescription = request.Data.Desc,
                Amount = request.Data.Amount,
                Currency = string.IsNullOrWhiteSpace(request.Data.Currency) ? "VND" : request.Data.Currency!,
                TransactionDateTime = ParseDateTime(request.Data.TransactionDateTime),
                CounterAccountName = request.Data.CounterAccountName,
                CounterAccountNumber = request.Data.CounterAccountNumber,
                CounterAccountBankName = request.Data.CounterAccountBankName,
                VirtualAccountName = request.Data.VirtualAccountName,
                VirtualAccountNumber = request.Data.VirtualAccountNumber,
                RawPayload = JsonSerializer.Serialize(request),
                Signature = request.Signature,
                IsSignatureValid = true
            };

            await _unitOfWork.PaymentTransactions.AddAsync(transaction);

            donation.PayOsPaymentLinkId = donation.PayOsPaymentLinkId ?? request.Data.PaymentLinkId;
            donation.TransactionRef = request.Data.Reference ?? donation.TransactionRef;
            donation.GatewayResponse = JsonSerializer.Serialize(request);

            if (!string.IsNullOrWhiteSpace(request.Data.CounterAccountName))
            {
                donation.DonorName = request.Data.CounterAccountName!;
            }

            var previousStatus = donation.Status;
            donation.Status = MapStatusFromPayOs(request, donation.ExpiresAt);
            if (donation.Status != DonationStatus.Pending)
            {
                donation.ProcessedAt = DateTime.UtcNow;
            }

            await _unitOfWork.Donations.UpdateAsync(donation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await AdjustCampaignProgressAsync(donation.CampaignId, donation.Amount, previousStatus, donation.Status, cancellationToken);
        }

        public async Task<Pagination<AdminDonationItemResponse>> GetAdminDonationsAsync(AdminDonationQueryRequest request, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.Donations.GetPagedAsync(
                request.PageIndex,
                request.PageSize,
                request.Status,
                request.CampaignId,
                request.Keyword,
                request.FromDate,
                request.ToDate,
                cancellationToken);

            var totalCount = await _unitOfWork.Donations.CountAsync(
                request.Status,
                request.CampaignId,
                request.Keyword,
                request.FromDate,
                request.ToDate,
                cancellationToken);

            var mapped = items.Select(d => new AdminDonationItemResponse
            {
                DonationId = d.DonationId,
                CampaignId = d.CampaignId,
                CampaignName = d.Campaign?.Name,
                DonorName = d.DonorName,
                Amount = d.Amount,
                Status = d.Status,
                OrderCode = d.PayOsOrderCode ?? 0,
                PaymentLinkId = d.PayOsPaymentLinkId,
                DonatedAt = d.DonatedAt,
                ExpiresAt = d.ExpiresAt,
                ProcessedAt = d.ProcessedAt
            }).ToList();

            return new Pagination<AdminDonationItemResponse>(mapped, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<AdminDonationDetailResponse> GetAdminDonationDetailAsync(Guid donationId, CancellationToken cancellationToken = default)
        {
            var donation = await _unitOfWork.Donations.GetDetailByIdAsync(donationId, cancellationToken);
            if (donation is null)
            {
                throw new DonationNotFoundException(donationId);
            }

            var transactions = await _unitOfWork.PaymentTransactions.GetByDonationIdAsync(donationId, cancellationToken);

            return new AdminDonationDetailResponse
            {
                DonationId = donation.DonationId,
                CampaignId = donation.CampaignId,
                CampaignName = donation.Campaign?.Name,
                DonorName = donation.DonorName,
                Amount = donation.Amount,
                Message = donation.Message,
                Status = donation.Status,
                OrderCode = donation.PayOsOrderCode ?? 0,
                PaymentLinkId = donation.PayOsPaymentLinkId,
                CheckoutUrl = donation.CheckoutUrl,
                DonatedAt = donation.DonatedAt,
                ExpiresAt = donation.ExpiresAt,
                ProcessedAt = donation.ProcessedAt,
                GatewayResponse = donation.GatewayResponse,
                Transactions = transactions.Select(t => new AdminPaymentTransactionResponse
                {
                    PaymentTransactionId = t.PaymentTransactionId,
                    Provider = t.Provider,
                    Reference = t.Reference,
                    EventCode = t.EventCode,
                    EventDescription = t.EventDescription,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    IsSignatureValid = t.IsSignatureValid,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };
        }

        public async Task<DonationStatusResponse> ReconcileAsync(Guid donationId, CancellationToken cancellationToken = default)
        {
            var donation = await _unitOfWork.Donations.GetByIdAsync(donationId);
            if (donation is null)
            {
                throw new DonationNotFoundException(donationId);
            }

            var identifier = !string.IsNullOrWhiteSpace(donation.PayOsPaymentLinkId)
                ? donation.PayOsPaymentLinkId!
                : (donation.PayOsOrderCode ?? 0).ToString(CultureInfo.InvariantCulture);

            var payOsInfo = await _payOsGateway.GetPaymentLinkInfoAsync(identifier, cancellationToken);

            var previousStatus = donation.Status;
            donation.Status = MapStatusFromPayOsStatus(payOsInfo.Status, donation.ExpiresAt);
            donation.GatewayResponse = JsonSerializer.Serialize(payOsInfo);
            donation.PayOsPaymentLinkId = payOsInfo.PaymentLinkId ?? donation.PayOsPaymentLinkId;
            if (donation.Status != DonationStatus.Pending)
            {
                donation.ProcessedAt = DateTime.UtcNow;
            }

            await _unitOfWork.Donations.UpdateAsync(donation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await AdjustCampaignProgressAsync(donation.CampaignId, donation.Amount, previousStatus, donation.Status, cancellationToken);

            return MapStatus(donation);
        }

        public async Task<DonationStatusResponse> CancelPendingAsync(Guid donationId, string? reason, CancellationToken cancellationToken = default)
        {
            var donation = await _unitOfWork.Donations.GetByIdAsync(donationId);
            if (donation is null)
            {
                throw new DonationNotFoundException(donationId);
            }

            if (donation.Status != DonationStatus.Pending)
            {
                throw new DonationInvalidStateException("Chỉ có thể huỷ donation đang ở trạng thái Pending.");
            }

            var identifier = !string.IsNullOrWhiteSpace(donation.PayOsPaymentLinkId)
                ? donation.PayOsPaymentLinkId!
                : (donation.PayOsOrderCode ?? 0).ToString(CultureInfo.InvariantCulture);

            var result = await _payOsGateway.CancelPaymentLinkAsync(identifier, reason, cancellationToken);
            var previousStatus = donation.Status;
            donation.Status = MapStatusFromPayOsStatus(result.Status, donation.ExpiresAt);
            donation.GatewayResponse = JsonSerializer.Serialize(result);
            donation.ProcessedAt = DateTime.UtcNow;

            await _unitOfWork.Donations.UpdateAsync(donation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await AdjustCampaignProgressAsync(donation.CampaignId, donation.Amount, previousStatus, donation.Status, cancellationToken);

            return MapStatus(donation);
        }

        public async Task<AdminDonationStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            var all = await _unitOfWork.Donations.GetAllAsync();

            return new AdminDonationStatsResponse
            {
                TotalAmount = all.Sum(x => x.Amount),
                TotalCount = all.Count,
                PendingCount = all.Count(x => x.Status == DonationStatus.Pending),
                CompletedCount = all.Count(x => x.Status == DonationStatus.Completed),
                FailedCount = all.Count(x => x.Status == DonationStatus.Failed),
                CancelledCount = all.Count(x => x.Status == DonationStatus.Cancelled),
                ExpiredCount = all.Count(x => x.Status == DonationStatus.Expired)
            };
        }

        public async Task<string> ExportCsvAsync(AdminDonationQueryRequest request, CancellationToken cancellationToken = default)
        {
            var donations = await _unitOfWork.Donations.GetAllFilteredAsync(
                request.Status,
                request.CampaignId,
                request.Keyword,
                request.FromDate,
                request.ToDate,
                cancellationToken);

            var sb = new StringBuilder();
            sb.AppendLine("DonationId,CampaignId,CampaignName,DonorName,Amount,Status,OrderCode,PaymentLinkId,DonatedAt,ProcessedAt");

            foreach (var d in donations)
            {
                sb.AppendLine(string.Join(",",
                    EscapeCsv(d.DonationId.ToString()),
                    EscapeCsv(d.CampaignId.ToString()),
                    EscapeCsv(d.Campaign?.Name),
                    EscapeCsv(d.DonorName),
                    EscapeCsv(d.Amount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(d.Status.ToString()),
                    EscapeCsv((d.PayOsOrderCode ?? 0).ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(d.PayOsPaymentLinkId),
                    EscapeCsv(d.DonatedAt.ToString("O")),
                    EscapeCsv(d.ProcessedAt?.ToString("O"))));
            }

            return sb.ToString();
        }

        private async Task AdjustCampaignProgressAsync(
            Guid campaignId,
            decimal amount,
            DonationStatus previousStatus,
            DonationStatus newStatus,
            CancellationToken cancellationToken)
        {
            if (previousStatus == newStatus)
            {
                return;
            }

            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new DonationCampaignNotFoundException(campaignId);

            if (previousStatus != DonationStatus.Completed && newStatus == DonationStatus.Completed)
            {
                // Transition into Completed: add the donated amount
                await _campaignService.UpdateProgressAsync(campaignId, CampaignResourceType.Money, amount, cancellationToken);
                campaign.BudgetTotal += amount;
                await _unitOfWork.Campaigns.UpdateAsync(campaign);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else if (previousStatus == DonationStatus.Completed && newStatus != DonationStatus.Completed)
            {
                // Transition out of Completed (refund, correction, cancel): subtract the donated amount
                if (campaign.BudgetTotal - amount < campaign.BudgetSpent)
                {
                    throw new DonationInvalidStateException("Không thể giảm BudgetTotal xuống thấp hơn BudgetSpent của campaign.");
                }

                await _campaignService.UpdateProgressAsync(campaignId, CampaignResourceType.Money, -amount, cancellationToken);
                campaign.BudgetTotal -= amount;
                await _unitOfWork.Campaigns.UpdateAsync(campaign);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private static string BuildDescription(long orderCode)
        {
            var value = $"DN{orderCode}";
            return value.Length > 25 ? value[..25] : value;
        }

        private static long BuildOrderCode(DateTime nowUtc)
        {
            var prefix = nowUtc.ToString("yyMMddHHmm", CultureInfo.InvariantCulture);
            var random = Random.Shared.Next(10, 99);
            return long.Parse(prefix + random, CultureInfo.InvariantCulture);
        }

        private static DonationStatusResponse MapStatus(Domain.Entities.Donation donation)
        {
            return new DonationStatusResponse
            {
                DonationId = donation.DonationId,
                OrderCode = donation.PayOsOrderCode ?? 0,
                Amount = donation.Amount,
                DonorName = donation.DonorName,
                Status = donation.Status,
                DonatedAt = donation.DonatedAt,
                ExpiresAt = donation.ExpiresAt,
                ProcessedAt = donation.ProcessedAt,
                CheckoutUrl = donation.CheckoutUrl
            };
        }

        private static DonationStatus MapStatusFromPayOs(PayOsWebhookRequest request, DateTime expiresAt)
        {
            var status = request.Data.Desc?.ToUpperInvariant();
            var code = request.Data.Code?.ToUpperInvariant();

            if (code == "00" && request.Success)
            {
                return DonationStatus.Completed;
            }

            if (status?.Contains("HUY") == true || status?.Contains("CANCEL") == true)
            {
                return DonationStatus.Cancelled;
            }

            if (DateTime.UtcNow > expiresAt)
            {
                return DonationStatus.Expired;
            }

            return DonationStatus.Failed;
        }

        private static DonationStatus MapStatusFromPayOsStatus(string? status, DateTime expiresAt)
        {
            var normalized = status?.ToUpperInvariant();
            return normalized switch
            {
                "PAID" => DonationStatus.Completed,
                "CANCELLED" => DonationStatus.Cancelled,
                "PENDING" when DateTime.UtcNow > expiresAt => DonationStatus.Expired,
                "PENDING" => DonationStatus.Pending,
                "PROCESSING" => DonationStatus.Pending,
                _ => DateTime.UtcNow > expiresAt ? DonationStatus.Expired : DonationStatus.Failed
            };
        }

        private static DateTime? ParseDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (DateTime.TryParse(value, out var parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            return null;
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}
