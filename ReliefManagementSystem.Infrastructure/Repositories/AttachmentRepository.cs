using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
    {
        public AttachmentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
