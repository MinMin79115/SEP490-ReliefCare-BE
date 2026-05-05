namespace ReliefManagementSystem.Application.Common.Models
{
    public class LlmAnalysisException : Exception
    {
        public LlmAnalysisException(string message, string? rawResponse = null, Exception? innerException = null)
            : base(message, innerException)
        {
            RawResponse = rawResponse;
        }

        public string? RawResponse { get; }
    }
}
