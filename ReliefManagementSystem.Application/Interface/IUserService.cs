using ReliefManagementSystem.Application.Features.VolunteerRequest.Request;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Response;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IUserService
    {
        public Task<VolunteerProfileResponse> CreateVolunteerProfileAsync(
            CreateVolunteerRequest request,
            CancellationToken cancellationToken = default);
    }
}
