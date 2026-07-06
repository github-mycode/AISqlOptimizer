namespace SqlOptimizer.Application.Options;

/// <summary>
/// Configuration options for OpenAI API
/// </summary>
public class OpenAIOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "OpenAI";

    /// <summary>
    /// OpenAI API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI API endpoint
    /// </summary>
    public string ApiEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    /// Model to use (e.g., gpt-4, gpt-3.5-turbo)
    /// </summary>
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    /// Maximum tokens in response
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// Temperature for response randomness (0.0 to 2.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry delay in milliseconds
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;
}
