using Grpc.Core;
using Grpc.Core.Interceptors;
using PeopleHub.Middleware;

namespace PeopleHub.Grpc;

public sealed class RequestIdClientInterceptor(IHttpContextAccessor httpContextAccessor) : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var requestId = httpContextAccessor.HttpContext?.GetRequestId();
        if (string.IsNullOrWhiteSpace(requestId))
        {
            return continuation(request, context);
        }

        var headers = context.Options.Headers ?? new Metadata();
        headers.Add(RequestId.HeaderName, requestId);

        return continuation(
            request,
            new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                context.Options.WithHeaders(headers)));
    }
}
