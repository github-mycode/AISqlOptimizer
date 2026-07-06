using System.Diagnostics;
using System.Text;

namespace SqlOptimizer.Api.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with execution time
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = Serilog.Log.ForContext<RequestResponseLoggingMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        // Log Request
        await LogRequestAsync(context, requestId);

        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            // Use a memory stream to capture the response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware
            await _next(context);

            stopwatch.Stop();

            // Log Response
            await LogResponseAsync(context, requestId, stopwatch.Elapsed);

            // Copy the response back to the original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.Error(ex,
                "Request {RequestId} failed after {ElapsedMs}ms. Path: {Path}, Method: {Method}",
                requestId,
                stopwatch.ElapsedMilliseconds,
                context.Request.Path,
                context.Request.Method);

            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string requestId)
    {
        try
        {
            var request = context.Request;

            // Enable buffering so we can read the body multiple times
            request.EnableBuffering();

            var requestBody = string.Empty;
            if (request.ContentLength > 0 && request.ContentLength < 10000) // Only log bodies < 10KB
            {
                using var reader = new StreamReader(
                    request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset stream position
            }

            _logger.Information(
                "HTTP Request {RequestId} started. Method: {Method}, Path: {Path}, QueryString: {QueryString}, ContentType: {ContentType}, ContentLength: {ContentLength}",
                requestId,
                request.Method,
                request.Path,
                request.QueryString.HasValue ? request.QueryString.Value : "none",
                request.ContentType ?? "none",
                request.ContentLength ?? 0);

            if (!string.IsNullOrEmpty(requestBody) && !request.Path.Value?.Contains("password", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.Debug("Request {RequestId} Body: {RequestBody}", requestId, requestBody);
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Error logging request {RequestId}", requestId);
        }
    }

    private async Task LogResponseAsync(HttpContext context, string requestId, TimeSpan elapsed)
    {
        try
        {
            var response = context.Response;

            var responseBody = string.Empty;
            if (response.Body.CanSeek && response.Body.Length > 0 && response.Body.Length < 10000)
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
                responseBody = await reader.ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
            }

            var logLevel = response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error :
                          response.StatusCode >= 400 ? Serilog.Events.LogEventLevel.Warning :
                          Serilog.Events.LogEventLevel.Information;

            _logger.Write(
                logLevel,
                "HTTP Response {RequestId} completed in {ElapsedMs}ms. StatusCode: {StatusCode}, ContentType: {ContentType}, ContentLength: {ContentLength}",
                requestId,
                elapsed.TotalMilliseconds,
                response.StatusCode,
                response.ContentType ?? "none",
                response.ContentLength ?? 0);

            if (!string.IsNullOrEmpty(responseBody) && logLevel != Serilog.Events.LogEventLevel.Error)
            {
                _logger.Debug("Response {RequestId} Body: {ResponseBody}", requestId, responseBody);
            }

            // Log slow requests
            if (elapsed.TotalSeconds > 5)
            {
                _logger.Warning(
                    "Slow request detected! {RequestId} took {ElapsedSeconds:F2}s. Path: {Path}",
                    requestId,
                    elapsed.TotalSeconds,
                    context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Error logging response {RequestId}", requestId);
        }
    }
}
