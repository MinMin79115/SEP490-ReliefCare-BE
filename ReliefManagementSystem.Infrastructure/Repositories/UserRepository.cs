using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class UserRepository
     : GenericRepository<ApplicationUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}
