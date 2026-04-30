using ReliefManagementSystem.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = ex.StatusCode,
                    message = ex.Message,
                    code = ex.ErrorCode,
                    errors = ex.Errors,
                    traceId = context.TraceIdentifier
                });
            }
            catch (AppException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message,
                    code = ex.ErrorCode,
                    traceId = context.TraceIdentifier,
                    statusCode = ex.StatusCode
                });
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateConcurrencyException concurrencyEx)
                {
                    var conflictEntries = concurrencyEx.Entries.Select(entry => new
                    {
                        Entity = entry.Metadata.ClrType.Name,
                        State = entry.State.ToString(),
                        PrimaryKeys = entry.Properties
                            .Where(p => p.Metadata.IsPrimaryKey())
                            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue?.ToString()),
                        ModifiedProperties = entry.Properties
                            .Where(p => p.IsModified)
                            .Select(p => p.Metadata.Name)
                            .ToList()
                    }).ToList();

                    _logger.LogError(
                        concurrencyEx,
                        "DbUpdateConcurrencyException. Path: {Path}, Method: {Method}, TraceId: {TraceId}, Entries: {@Entries}",
                        context.Request.Path,
                        context.Request.Method,
                        context.TraceIdentifier,
                        conflictEntries);
                }
                else
                {
                    _logger.LogError(
                        ex,
                        "Unhandled exception. Path: {Path}, Method: {Method}, TraceId: {TraceId}",
                        context.Request.Path,
                        context.Request.Method,
                        context.TraceIdentifier);
                }

                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Internal Server Error",
                    detail = ex.Message,
                    innerException = ex.InnerException?.Message,
                    traceId = context.TraceIdentifier
                });
            }

        }
    }
}
