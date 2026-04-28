using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class SupplyTransferRepository : GenericRepository<SupplyTransfer>, ISupplyTransferRepository
    {
        public SupplyTransferRepository(ApplicationDbContext context) : base(context) { }

        public async Task<SupplyTransfer?> GetByIdWithDetailsAsync(Guid transferId, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyTransfers
                .Include(t => t.SourceStation)
                .Include(t => t.DestinationStation)
                .Include(t => t.RequestedByUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.DriverUser)
                .Include(t => t.Vehicle)
                .Include(t => t.SupplyTransferVehicles).ThenInclude(v => v.Vehicle).ThenInclude(v => v.VehicleType)
                .Include(t => t.SupplyTransferVehicles).ThenInclude(v => v.DriverUser)
                .Include(t => t.Documents)
                .Include(t => t.Items).ThenInclude(i => i.SupplyItem)
                .Include(t => t.InventoryTransactions)
                .FirstOrDefaultAsync(t => t.SupplyTransferId == transferId, cancellationToken);
        }

        public async Task<IReadOnlyList<SupplyTransfer>> GetByStatusAsync(SupplyTransferStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyTransfers
                .AsNoTracking()
                .Include(t => t.SourceStation)
                .Include(t => t.DestinationStation)
                .Include(t => t.RequestedByUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.Documents)
                .Include(t => t.Items)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SupplyTransfer>> GetBySourceStationAsync(Guid stationId, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyTransfers
                .AsNoTracking()
                .Include(t => t.SourceStation)
                .Include(t => t.DestinationStation)
                .Include(t => t.RequestedByUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.Documents)
                .Include(t => t.Items)
                .Where(t => t.SourceStationId == stationId)
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SupplyTransfer>> GetByDestinationStationAsync(Guid stationId, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyTransfers
                .AsNoTracking()
                .Include(t => t.SourceStation)
                .Include(t => t.DestinationStation)
                .Include(t => t.RequestedByUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.Documents)
                .Include(t => t.Items)
                .Where(t => t.DestinationStationId == stationId)
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountTodayAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            return await _context.SupplyTransfers.CountAsync(t => t.RequestedAt >= today && t.RequestedAt < tomorrow, cancellationToken);
        }
    }
}
