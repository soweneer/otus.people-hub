using Grpc.Core;
using PeopleHub.Counters.Grpc;
using PeopleHub.Model;
using CountersClient = PeopleHub.Counters.Grpc.Counters;

namespace PeopleHub.Counters;

internal sealed class CounterService(CountersClient.CountersClient client, ILogger<CounterService> logger) : ICounterService
{
    private static readonly UnreadCountersResponse Empty = new([], 0);

    public async Task<UnreadCountersResponse> GetUnreadAsync(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetCountersAsync(
                new CountersRequest { UserId = userId },
                cancellationToken: cancellationToken);

            return new UnreadCountersResponse(
                response.Counters.Select(counter => new UnreadCounterResponse(counter.PartnerId, counter.Count)).ToArray(),
                response.Total);
        }
        catch (RpcException exception)
        {
            logger.LogWarning("Сервис счётчиков недоступен ({StatusCode}), отдаём пустые счётчики", exception.StatusCode);

            return Empty;
        }
    }
}
