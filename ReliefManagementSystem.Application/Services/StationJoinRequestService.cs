using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions;
using ReliefManagementSystem.Application.Common.Exceptions.Team;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.StationJoinRequest.DTOs.Request;
using ReliefManagementSystem.Application.Features.StationJoinRequest.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class StationJoinRequestService : IStationJoinRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StationJoinRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StationJoinRequestResponse> CreateRequestAsync(
            CreateStationJoinRequestRequest request,
            Guid leaderId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId);
            if (team == null)
                throw new TeamNotFoundException(request.TeamId);

            if (!team.LeaderId.HasValue || team.LeaderId.Value != leaderId)
                throw new TeamLeaderMismatchException();

            if (team.Status != TeamStatus.Active)
                throw new TeamInactiveException(team.Name);

            var station = await _unitOfWork.ReliefStations.GetByIdAsync(request.ReliefStationId);
            if (station == null)
                throw new ReliefStationNotFoundException(request.ReliefStationId);

            if (station.Level != ReliefStationLevel.Provincial)
                throw new InvalidLocationForProvincialStationException();

            if (station.ReliefStationStatus != ReliefStationStatus.Active)
                throw new ReliefStationInactiveException();

            var existingPending = await _unitOfWork.StationJoinRequests
                .GetExistingPendingRequestAsync(request.TeamId, request.ReliefStationId, cancellationToken);
            if (existingPending != null)
                throw new StationJoinRequestAlreadyPendingException();

            var joinRequest = new StationJoinRequest
            {
                StationJoinRequestId = Guid.NewGuid(),
                TeamId = request.TeamId,
                ReliefStationId = request.ReliefStationId,
                RequestedByLeaderId = leaderId,
                Description = request.Description,
                Status = StationJoinRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            await _unitOfWork.StationJoinRequests.AddAsync(joinRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = await _unitOfWork.StationJoinRequests.GetByIdWithDetailsAsync(joinRequest.StationJoinRequestId, cancellationToken);
            return MapToResponse(result!);
        }

        public async Task<StationJoinRequestResponse> GetByIdAsync(Guid requestId, CancellationToken cancellationToken)
        {
            var request = await _unitOfWork.StationJoinRequests.GetByIdWithDetailsAsync(requestId, cancellationToken);
            if (request == null)
                throw new StationJoinRequestNotFoundException(requestId);

            return MapToResponse(request);
        }

        public async Task<Pagination<StationJoinRequestResponse>> GetMyRequestsAsync(
            Guid leaderId,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.StationJoinRequests.GetQueryableWithDetails()
                .Where(x => x.RequestedByLeaderId == leaderId)
                .OrderByDescending(x => x.RequestedAt);

            var paged = await Pagination<StationJoinRequest>.ToPagedList(query, pageIndex, pageSize);
            var items = paged.Items!.Select(MapToResponse).ToList();

            return new Pagination<StationJoinRequestResponse>(items, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<Pagination<StationJoinRequestResponse>> GetPendingByStationAsync(
            Guid stationId,
            Guid moderatorId,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var head = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(stationId, cancellationToken);
            if (head == null || head.UserId != moderatorId)
                throw new OnlyStationHeadCanManageAssignmentsException();

            var query = _unitOfWork.StationJoinRequests.GetQueryableWithDetails()
                .Where(x => x.ReliefStationId == stationId && x.Status == StationJoinRequestStatus.Pending)
                .OrderByDescending(x => x.RequestedAt);

            var paged = await Pagination<StationJoinRequest>.ToPagedList(query, pageIndex, pageSize);
            var items = paged.Items!.Select(MapToResponse).ToList();

            return new Pagination<StationJoinRequestResponse>(items, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<StationJoinRequestResponse> ApproveAsync(
            Guid requestId,
            Guid moderatorId,
            ReviewStationJoinRequestRequest request,
            CancellationToken cancellationToken)
        {
            var joinRequest = await _unitOfWork.StationJoinRequests.GetByIdWithDetailsAsync(requestId, cancellationToken);
            if (joinRequest == null)
                throw new StationJoinRequestNotFoundException(requestId);

            var head = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(joinRequest.ReliefStationId, cancellationToken);
            if (head == null || head.UserId != moderatorId)
                throw new OnlyStationHeadCanManageAssignmentsException();

            if (joinRequest.Status != StationJoinRequestStatus.Pending)
                throw new ReliefStationAssignmentNotPendingException();

            var assignment = await _unitOfWork.ReliefStationTeams
                .GetByStationAndTeamAsync(joinRequest.ReliefStationId, joinRequest.TeamId, cancellationToken);

            if (assignment == null)
            {
                assignment = new ReliefStationTeam
                {
                    ReliefStationTeamId = Guid.NewGuid(),
                    ReliefStationId = joinRequest.ReliefStationId,
                    TeamId = joinRequest.TeamId,
                    Status = ReliefTeamAssignmentStatus.Approved,
                    Description = joinRequest.Description,
                    JoinedAt = DateTime.UtcNow
                };
                await _unitOfWork.ReliefStationTeams.AddAsync(assignment);
            }
            else
            {
                assignment.Status = ReliefTeamAssignmentStatus.Approved;
                assignment.Description = joinRequest.Description ?? assignment.Description;
                assignment.RejectionReason = null;
                assignment.JoinedAt ??= DateTime.UtcNow;
                await _unitOfWork.ReliefStationTeams.UpdateAsync(assignment);
            }

            joinRequest.Status = StationJoinRequestStatus.Approved;
            joinRequest.ReviewNote = request.ReviewNote;
            joinRequest.ReviewedAt = DateTime.UtcNow;
            joinRequest.ReviewedByModeratorId = moderatorId;
            joinRequest.ApprovedAt = DateTime.UtcNow;
            await _unitOfWork.StationJoinRequests.UpdateAsync(joinRequest);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(joinRequest);
        }

        public async Task<StationJoinRequestResponse> RejectAsync(
            Guid requestId,
            Guid moderatorId,
            ReviewStationJoinRequestRequest request,
            CancellationToken cancellationToken)
        {
            var joinRequest = await _unitOfWork.StationJoinRequests.GetByIdWithDetailsAsync(requestId, cancellationToken);
            if (joinRequest == null)
                throw new StationJoinRequestNotFoundException(requestId);

            var head = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(joinRequest.ReliefStationId, cancellationToken);
            if (head == null || head.UserId != moderatorId)
                throw new OnlyStationHeadCanManageAssignmentsException();

            if (joinRequest.Status != StationJoinRequestStatus.Pending)
                throw new ReliefStationAssignmentNotPendingException();

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                throw new RejectionReasonRequiredException();

            joinRequest.Status = StationJoinRequestStatus.Rejected;
            joinRequest.ReviewNote = request.ReviewNote;
            joinRequest.RejectionReason = request.RejectionReason;
            joinRequest.ReviewedAt = DateTime.UtcNow;
            joinRequest.ReviewedByModeratorId = moderatorId;
            joinRequest.RejectedAt = DateTime.UtcNow;

            await _unitOfWork.StationJoinRequests.UpdateAsync(joinRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(joinRequest);
        }

        public async Task<bool> CancelAsync(Guid requestId, Guid leaderId, CancellationToken cancellationToken)
        {
            var joinRequest = await _unitOfWork.StationJoinRequests.GetByIdWithDetailsAsync(requestId, cancellationToken);
            if (joinRequest == null)
                throw new StationJoinRequestNotFoundException(requestId);

            if (joinRequest.RequestedByLeaderId != leaderId)
                throw new TeamLeaderMismatchException();

            if (joinRequest.Status != StationJoinRequestStatus.Pending)
                throw new ReliefStationAssignmentNotPendingException();

            joinRequest.Status = StationJoinRequestStatus.Cancelled;
            joinRequest.CancelledAt = DateTime.UtcNow;

            await _unitOfWork.StationJoinRequests.UpdateAsync(joinRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static StationJoinRequestResponse MapToResponse(StationJoinRequest request)
        {
            return new StationJoinRequestResponse
            {
                StationJoinRequestId = request.StationJoinRequestId,
                TeamId = request.TeamId,
                TeamName = request.Team?.Name ?? string.Empty,
                ReliefStationId = request.ReliefStationId,
                ReliefStationName = request.ReliefStation?.Name ?? string.Empty,
                RequestedByLeaderId = request.RequestedByLeaderId,
                RequestedByLeaderName = request.RequestedByLeader?.DisplayName ?? request.RequestedByLeader?.UserName ?? string.Empty,
                Status = request.Status,
                Description = request.Description,
                RejectionReason = request.RejectionReason,
                ReviewNote = request.ReviewNote,
                RequestedAt = request.RequestedAt,
                ReviewedAt = request.ReviewedAt,
                ReviewedByModeratorId = request.ReviewedByModeratorId,
                ReviewedByModeratorName = request.ReviewedByModerator?.DisplayName ?? request.ReviewedByModerator?.UserName,
                ApprovedAt = request.ApprovedAt,
                RejectedAt = request.RejectedAt,
                CancelledAt = request.CancelledAt
            };
        }
    }
}
