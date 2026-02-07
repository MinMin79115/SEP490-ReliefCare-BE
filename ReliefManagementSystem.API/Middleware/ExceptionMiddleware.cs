using ReliefManagementSystem.Application.Common.Exceptions;

namespace ReliefManagementSystem.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Internal Server Error",
                    detail = ex.Message
                });
            }

        }
    }
}
