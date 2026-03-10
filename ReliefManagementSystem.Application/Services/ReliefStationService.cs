using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.ReliefStation.Dtos;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    /// <summary>
    /// Business logic for ReliefStation and team assignment management.
    /// </summary>
    public class ReliefStationService : IReliefStationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReliefStationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _userManager = userManager;
        }

        public async Task<Guid> CreateProvincialReliefStationAsync(CreateProvincialReliefStationRequest request, CancellationToken cancellationToken)
        {
           

            // 2️⃣ Validate coordinates
            if (request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180)
            {
                throw new InvalidCoordinatesException();
            }

            // 3️⃣ Check Location exists
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);

            if (location == null)
            {
                throw new LocationNotFoundException();
            }

            // 4️⃣ Validate location level (optional nếu Location có Level)
            if (location.Level != LocationLevel.Province)
            {
                throw new InvalidLocationForProvincialStationException();
            }


            if (await _unitOfWork.ReliefStations.ExistsByNameAsync(request.Name))
            {
                throw new StationNameAlreadyExistsException(request.Name);
            }


            if (await _unitOfWork.ReliefStations.ExistsProvincialStationInLocationAsync(request.LocationId))
            {
                throw new DuplicateReliefStationLocationException();
            }

            try
            {

                var station = new ReliefStation
                {
                    LocationId = request.LocationId,
                    Name = request.Name,
                    Address = request.Address,
                    ContactNumber = request.ContactNumber,
                    Longitude = request.Longitude,
                    Latitude = request.Latitude,

                    Level = ReliefStationLevel.Provincial,
                    Status = ReliefStationStatus.Active,
                    IsActive = true
                };

                var inventory = new Inventory
                {
                    Level = InventoryLevel.Provincial,
                    Status = EntityStatus.Active,
                    ReliefStation = station

                };

                await _unitOfWork.ReliefStations.AddAsync(station);
                await _unitOfWork.Inventories.AddAsync(inventory);

                var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (result <= 0)
                {
                    throw new ReliefStationCreationFailedException();
                }

                return station.ReliefStationId;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InventoryCreationFailedException();
            }
        }
    }
}
