namespace ReliefManagementSystem.Application.Common.Models
{
    public class LlmProviderSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ProviderName { get; set; } = "OpenAI-Compatible";
        public string DefaultModel { get; set; } = string.Empty;
        public string ChatCompletionsPath { get; set; } = "/chat/completions";
        public double Temperature { get; set; } = 0.2;
        public int MaxTokens { get; set; } = 600;
    }
}
