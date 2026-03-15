using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.PriorityCriteriaExceptions
{
    public class PriorityCriteriaNotFoundException : AppException
    {
        public PriorityCriteriaNotFoundException(Guid id) 
            : base($"Tiêu chí ưu tiên với ID {id} không tồn tại.","NOT_FOUND_ID_PRIORITY",400)
        {
        }
    }
}
