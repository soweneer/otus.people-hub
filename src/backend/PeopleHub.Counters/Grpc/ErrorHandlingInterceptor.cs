using Grpc.Core;
using Grpc.Core.Interceptors;
using StackExchange.Redis;

namespace PeopleHub.Counters.Grpc;

public sealed class ErrorHandlingInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "Запрос отменён"));
        }
        catch (RedisConnectionException exception)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, exception.Message));
        }
        catch (RedisTimeoutException exception)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, exception.Message));
        }
    }
}
