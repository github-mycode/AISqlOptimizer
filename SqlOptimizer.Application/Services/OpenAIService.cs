using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;
using SqlOptimizer.Application.Options;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service for integrating with OpenAI API
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenAIOptions> options,
        ILogger<OpenAIService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<StoredProcedureAnalysisDto> AnalyzeStoredProcedureAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting OpenAI analysis");

        // Validate API key
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogError("OpenAI API key is not configured");
            return new StoredProcedureAnalysisDto
            {
                Success = false,
                ErrorMessage = "OpenAI API key is not configured. Please add it to appsettings.json"
            };
        }

        var attempt = 0;
        Exception? lastException = null;

        while (attempt < _options.MaxRetryAttempts)
        {
            attempt++;

            try
            {
                _logger.LogDebug("Attempt {Attempt} of {MaxAttempts}", attempt, _options.MaxRetryAttempts);

                var response = await CallOpenAIApiAsync(prompt, cancellationToken);

                _logger.LogInformation("Successfully received OpenAI response");

                return response;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "OpenAI request timed out on attempt {Attempt}", attempt);
                lastException = ex;

                if (attempt < _options.MaxRetryAttempts)
                {
                    await Task.Delay(_options.RetryDelayMilliseconds * attempt, cancellationToken);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP error on attempt {Attempt}: {Message}", attempt, ex.Message);
                lastException = ex;

                if (attempt < _options.MaxRetryAttempts)
                {
                    await Task.Delay(_options.RetryDelayMilliseconds * attempt, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling OpenAI API: {Message}", ex.Message);
                
                return new StoredProcedureAnalysisDto
                {
                    Success = false,
                    ErrorMessage = $"Error analyzing stored procedure: {ex.Message}"
                };
            }
        }

        _logger.LogError(lastException, "Failed to call OpenAI API after {Attempts} attempts", _options.MaxRetryAttempts);

        return new StoredProcedureAnalysisDto
        {
            Success = false,
            ErrorMessage = $"Failed to analyze after {_options.MaxRetryAttempts} attempts. Last error: {lastException?.Message}"
        };
    }

    /// <summary>
    /// Calls the OpenAI API
    /// </summary>
    private async Task<StoredProcedureAnalysisDto> CallOpenAIApiAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("OpenAI");
        httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = _options.Temperature,
            max_tokens = _options.MaxTokens,
            response_format = new { type = "json_object" } // Request JSON response
        };

        _logger.LogDebug("Sending request to OpenAI API with model: {Model}", _options.Model);

        var response = await httpClient.PostAsJsonAsync(
            _options.ApiEndpoint,
            requestBody,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "OpenAI API returned error status {StatusCode}: {Error}",
                response.StatusCode,
                errorContent);

            throw new HttpRequestException(
                $"OpenAI API returned {response.StatusCode}: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Received response from OpenAI ({Length} characters)", responseContent.Length);

        // Parse OpenAI response
        var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

        if (openAiResponse?.Choices == null || !openAiResponse.Choices.Any())
        {
            throw new InvalidOperationException("OpenAI response does not contain any choices");
        }

        var aiContent = openAiResponse.Choices[0].Message?.Content;

        if (string.IsNullOrEmpty(aiContent))
        {
            throw new InvalidOperationException("OpenAI response content is empty");
        }

        _logger.LogDebug("Parsing AI response content");

        // Parse the JSON response from AI
        return ParseAIResponse(aiContent);
    }

    /// <summary>
    /// Parses the AI response JSON into our DTO
    /// </summary>
    private StoredProcedureAnalysisDto ParseAIResponse(string jsonContent)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var aiResponse = JsonSerializer.Deserialize<AIAnalysisResponse>(jsonContent, options);

            if (aiResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize AI response");
            }

            return new StoredProcedureAnalysisDto
            {
                Success = true,
                PerformanceScore = aiResponse.PerformanceScore,
                Severity = aiResponse.Severity ?? "Unknown",
                Summary = aiResponse.Summary ?? "No summary provided",
                Issues = aiResponse.Issues?.Select(i => new AnalysisIssueDto
                {
                    Type = i.Type ?? "Unknown",
                    Severity = i.Severity ?? "Unknown",
                    Description = i.Description ?? "",
                    LineNumber = i.LineNumber,
                    CodeSnippet = i.CodeSnippet
                }).ToList() ?? new List<AnalysisIssueDto>(),
                Recommendations = aiResponse.Recommendations?.Select(r => new RecommendationDto
                {
                    Priority = r.Priority ?? "Medium",
                    Title = r.Title ?? "",
                    Description = r.Description ?? "",
                    ExpectedImpact = r.ExpectedImpact,
                    ImplementationSteps = r.ImplementationSteps ?? new List<string>(),
                    SqlCode = r.SqlCode
                }).ToList() ?? new List<RecommendationDto>(),
                OptimizedCode = aiResponse.OptimizedCode
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI response JSON: {Content}", jsonContent);

            return new StoredProcedureAnalysisDto
            {
                Success = false,
                ErrorMessage = $"Failed to parse AI response: {ex.Message}",
                Summary = "AI returned an invalid response format"
            };
        }
    }

    #region OpenAI API Response Models

    /// <summary>
    /// OpenAI API response structure
    /// </summary>
    private class OpenAIResponse
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Created { get; set; }
        public string? Model { get; set; }
        public List<Choice>? Choices { get; set; }
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        public int Index { get; set; }
        public Message? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    private class Message
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }

    private class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// AI analysis response structure (what we expect from the AI)
    /// </summary>
    private class AIAnalysisResponse
    {
        public int? PerformanceScore { get; set; }
        public string? Severity { get; set; }
        public string? Summary { get; set; }
        public List<AIIssue>? Issues { get; set; }
        public List<AIRecommendation>? Recommendations { get; set; }
        public string? OptimizedCode { get; set; }
    }

    private class AIIssue
    {
        public string? Type { get; set; }
        public string? Severity { get; set; }
        public string? Description { get; set; }
        public int? LineNumber { get; set; }
        public string? CodeSnippet { get; set; }
    }

    private class AIRecommendation
    {
        public string? Priority { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ExpectedImpact { get; set; }
        public List<string>? ImplementationSteps { get; set; }
        public string? SqlCode { get; set; }
    }

    #endregion
}
