using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.PriorityCriteriaExceptions
{
    public class DuplicatePriorityCriteriaCodeException : ConflictException
    {
        public DuplicatePriorityCriteriaCodeException(string code) 
            : base($"Mã tiêu chí ưu tiên '{code}' đã tồn tại trong hệ thống.")
        {
        }
    }
}
