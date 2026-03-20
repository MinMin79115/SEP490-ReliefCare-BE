using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Application.Features.Location.Dtos;

namespace ReliefManagementSystem.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<LocationDto>> GetRegionsAsync()
        {
            var entities = await _unitOfWork.Locations
                .GetByLevelAsync(LocationLevel.Region);

            return MapToDtoList(entities);
        }

        public async Task<List<LocationDto>> GetProvincesAsync()
        {
            var entities = await _unitOfWork.Locations
                .GetByLevelAsync(LocationLevel.Province);

            return MapToDtoList(entities);
        }

        public async Task<List<LocationDto>> GetCommunesAsync()
        {
            var entities = await _unitOfWork.Locations
                .GetByLevelAsync(LocationLevel.Commune);

            return MapToDtoList(entities);
        }

        public async Task<List<LocationDto>> GetProvincesByRegionAsync(Guid regionId)
        {
            var entities = await _unitOfWork.Locations
                .GetChildrenByParentAsync(regionId, LocationLevel.Province);

            return MapToDtoList(entities);
        }

        public async Task<List<LocationDto>> GetCommunesByProvinceAsync(Guid provinceId)
        {
            var entities = await _unitOfWork.Locations
                .GetChildrenByParentAsync(provinceId, LocationLevel.Commune);

            return MapToDtoList(entities);
        }

        public async Task<List<LocationDto>> SearchByPathAsync(string path)
        {
            var entities = await _unitOfWork.Locations
                .SearchByPathAsync(path);

            return MapToDtoList(entities);
        }

        public async Task<List<LocationTreeDto>> GetLocationTreeAsync()
        {
            var locations = await _unitOfWork.Locations.GetAllActiveAsync();

            var lookup = locations.ToLookup(x => x.ParentId);

            List<LocationTreeDto> BuildTree(Guid? parentId)
            {
                return lookup[parentId]
                    .OrderBy(x => x.Name)
                    .Select(x => new LocationTreeDto
                    {
                        Id = x.LocationId,
                        Name = x.Name,
                        Level = (int)x.Level,
                        Children = BuildTree(x.LocationId)
                    }).ToList();
            }

            return BuildTree(null);
        }

        #region MAPPING

        private static LocationDto MapToDto(Location location)
        {
            return new LocationDto
            {
                Id = location.LocationId,
                Name = location.Name,
                FullName = location.FullName,
                Path = location.Path,
                Level = (int)location.Level
            };
        }

        private static List<LocationDto> MapToDtoList(IEnumerable<Location> locations)
        {
            return locations
                .Select(MapToDto)
                .ToList();
        }

        #endregion
    }
}
