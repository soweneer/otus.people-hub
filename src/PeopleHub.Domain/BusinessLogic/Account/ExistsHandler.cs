using MediatR;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Domain.BusinessLogic.Account;

public sealed record ExistsRequest(string Email): IRequest<bool>;

public sealed class ExistsHandler(DbClient dbClient) : IRequestHandler<ExistsRequest, bool>
{
    public async Task<bool> Handle(ExistsRequest request, CancellationToken cancellationToken)
    {
        var query = $"SELECT COUNT(*) FROM \"Accounts\" WHERE \"Email\" = '{request.Email}'";
        var count = await dbClient.TryGetIntAsync(query);
        return count > 0;
    }
}
