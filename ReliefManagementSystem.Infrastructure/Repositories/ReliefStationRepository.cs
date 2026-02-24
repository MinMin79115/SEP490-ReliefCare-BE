using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class ReliefStationRepository : GenericRepository<ReliefStation>, IReliefStationRepository
    {
        public ReliefStationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
