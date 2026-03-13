using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.PriorityCriteriaExceptions
{
    public class PriorityCriteriaNotFoundException : NotFoundException
    {
        public PriorityCriteriaNotFoundException(Guid id) 
            : base($"Tiêu chí ưu tiên với ID {id} không tồn tại.")
        {
        }
    }
}
