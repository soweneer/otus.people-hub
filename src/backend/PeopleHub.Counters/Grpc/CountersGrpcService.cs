using Grpc.Core;
using PeopleHub.Counters.Storage;

namespace PeopleHub.Counters.Grpc;

internal sealed class CountersGrpcService(ICounterStore store) : Counters.CountersBase
{
    public override async Task<CountersResponse> GetCounters(CountersRequest request, ServerCallContext context)
    {
        var snapshot = await store.GetAsync(request.UserId, context.CancellationToken);

        var response = new CountersResponse { Total = snapshot.Total };
        foreach (var counter in snapshot.Counters)
        {
            response.Counters.Add(new PartnerCounter { PartnerId = counter.PartnerId, Count = counter.Count });
        }

        return response;
    }

    public override async Task<TotalResponse> GetTotal(CountersRequest request, ServerCallContext context)
    {
        var total = await store.GetTotalAsync(request.UserId, context.CancellationToken);

        return new TotalResponse { Total = total };
    }
}
