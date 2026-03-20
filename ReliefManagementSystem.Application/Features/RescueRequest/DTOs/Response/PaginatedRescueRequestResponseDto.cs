using System;
using System.Collections.Generic;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    /// <summary>DTO phản hồi danh sách yêu cầu cứu hộ với phân trang</summary>
    public class PaginatedRescueRequestResponseDto
    {
        public List<RescueRequestResponseDto> Data { get; set; } = new();

        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}