using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace PeopleHub.Chats.Grpc;

public sealed class RequestIdServerInterceptor(ILogger<RequestIdServerInterceptor> logger) : Interceptor
{
    public const string HeaderName = "x-request-id";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var requestId = context.RequestHeaders.GetValue(HeaderName);
        if (string.IsNullOrWhiteSpace(requestId))
        {
            requestId = Guid.NewGuid().ToString("N");
        }

        context.UserState[HeaderName] = requestId;
        context.ResponseTrailers.Add(HeaderName, requestId);

        using var scope = logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = requestId });

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await continuation(request, context);

            logger.LogInformation(
                "{Method} завершён со статусом {StatusCode} за {ElapsedMs} мс",
                context.Method,
                StatusCode.OK,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (RpcException exception)
        {
            logger.LogWarning(
                "{Method} завершён со статусом {StatusCode} за {ElapsedMs} мс: {Detail}",
                context.Method,
                exception.StatusCode,
                stopwatch.ElapsedMilliseconds,
                exception.Status.Detail);

            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "{Method} завершён с необработанной ошибкой за {ElapsedMs} мс",
                context.Method,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
