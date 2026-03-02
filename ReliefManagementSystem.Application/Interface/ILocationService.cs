using ReliefManagementSystem.Application.Features.Location.Dtos;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetRegionsAsync();
        Task<List<LocationDto>> GetProvincesAsync();
        Task<List<LocationDto>> GetCommunesAsync();

        Task<List<LocationDto>> GetProvincesByRegionAsync(Guid regionId);
        Task<List<LocationDto>> GetCommunesByProvinceAsync(Guid provinceId);

        Task<List<LocationDto>> SearchByPathAsync(string path);
        Task<List<LocationTreeDto>> GetLocationTreeAsync();
    }
}
