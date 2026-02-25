using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.Application.Services
{
    public class ReliefStationService : IReliefStationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ReliefStationService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        //public async Task<IReadOnlyList<ReliefStationResponse>> GetAllReliefStationsAsync(
        //    CancellationToken cancellationToken)
        //{
        //    var reliefStations = await _unitOfWork.ReliefStations.GetAllAsync();
        //    return reliefStations
        //        .Select(reliefStation => new ReliefStationResponse
        //        {
        //            ReliefStationId = reliefStation.ReliefStationId,
        //            Name = reliefStation.Name,
        //            Location = reliefStation.Location,
        //            Capacity = reliefStation.Capacity,
        //            CurrentOccupancy = reliefStation.CurrentOccupancy
        //        })
        //        .ToList();
        //}


    }
}
