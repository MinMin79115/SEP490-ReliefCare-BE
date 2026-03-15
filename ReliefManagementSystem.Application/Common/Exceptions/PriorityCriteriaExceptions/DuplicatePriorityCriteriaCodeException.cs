using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.PriorityCriteriaExceptions
{
    public class DuplicatePriorityCriteriaCodeException : AppException
    {
        public DuplicatePriorityCriteriaCodeException(string code) 
            : base($"Mã tiêu chí ưu tiên '{code}' đã tồn tại trong hệ thống.", "DUPLICATED_CODE_PRIORITY",400)
        {
        }
    }
}
