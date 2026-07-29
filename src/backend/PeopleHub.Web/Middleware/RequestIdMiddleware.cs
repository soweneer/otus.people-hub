using System.Diagnostics;

namespace PeopleHub.Middleware;

public sealed class RequestIdMiddleware(RequestDelegate next, ILogger<RequestIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers[RequestId.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(requestId))
        {
            requestId = RequestId.New();
        }

        context.SetRequestId(requestId);
        context.Response.Headers[RequestId.HeaderName] = requestId;

        using var scope = logger.BeginScope("x-request-id:{XRequestId}", requestId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            logger.LogInformation(
                "{Method} {Path} завершён со статусом {StatusCode} за {ElapsedMs} мс",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
