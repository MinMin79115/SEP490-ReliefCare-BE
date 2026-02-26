using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("regions")]
        public async Task<IActionResult> GetRegions()
        {
            var result = await _locationService.GetRegionsAsync();
            return Ok(result);
        }


        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var result = await _locationService.GetProvincesAsync();
            return Ok(result);
        }


        [HttpGet("communes")]
        public async Task<IActionResult> GetCommunes()
        {
            var result = await _locationService.GetCommunesAsync();
            return Ok(result);
        }


        [HttpGet("regions/{regionId}/provinces")]
        public async Task<IActionResult> GetProvincesByRegion(Guid regionId)
        {
            var result = await _locationService.GetProvincesByRegionAsync(regionId);
            return Ok(result);
        }

        [HttpGet("provinces/{provinceId}/communes")]
        public async Task<IActionResult> GetCommunesByProvince(Guid provinceId)
        {
            var result = await _locationService.GetCommunesByProvinceAsync(provinceId);
            return Ok(result);
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            var result = await _locationService.GetLocationTreeAsync();
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string path)
        {
            var result = await _locationService.SearchByPathAsync(path);
            return Ok(result);
        }

    }
}
