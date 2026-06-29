using System.Net;
using System.Text.Json;

namespace AustimAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next) { _next = next; }

        public async Task Invoke(HttpContext context)
        {
            try { await _next(context); }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = ex.Message }));
            }
        }
    }
}