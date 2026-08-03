using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace PeopleHub.Chats.Grpc;

public sealed class MetricsInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var method = ExtractMethodName(context.Method);
        var status = StatusCode.OK;
        var stopwatch = Stopwatch.StartNew();

        ChatsMetrics.InFlight.WithLabels(method).Inc();

        try
        {
            return await continuation(request, context);
        }
        catch (RpcException exception)
        {
            status = exception.StatusCode;

            throw;
        }
        catch (Exception)
        {
            status = StatusCode.Unknown;

            throw;
        }
        finally
        {
            stopwatch.Stop();

            ChatsMetrics.InFlight.WithLabels(method).Dec();
            ChatsMetrics.Duration.WithLabels(method).Observe(stopwatch.Elapsed.TotalSeconds);
            ChatsMetrics.Requests.WithLabels(method, status.ToString()).Inc();
        }
    }

    private static string ExtractMethodName(string fullMethod)
    {
        var separator = fullMethod.LastIndexOf('/');

        return separator < 0 ? fullMethod : fullMethod[(separator + 1)..];
    }
}
