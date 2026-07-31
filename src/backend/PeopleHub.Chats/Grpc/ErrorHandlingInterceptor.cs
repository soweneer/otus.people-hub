using Grpc.Core;
using Grpc.Core.Interceptors;
using PeopleHub.Chats.Domain;
using PeopleHub.Chats.Tarantool;

namespace PeopleHub.Chats.Grpc;

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
        catch (DomainException exception)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, exception.Message));
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "Запрос отменён"));
        }
        catch (TarantoolException exception)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, exception.Message));
        }
    }
}
