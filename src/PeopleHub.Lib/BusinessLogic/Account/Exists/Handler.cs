using MediatR;
using PeopleHub.Dal.Infrastructure.Db;

namespace PeopleHub.Lib.BusinessLogic.Account.Exists;

public sealed class Handler : IRequestHandler<Request, bool>
{
    private readonly DbClient _dbClient;

    public Handler(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    public async Task<bool> Handle(Request request, CancellationToken cancellationToken)
    {
        var query = $"SELECT COUNT(*) FROM \"Accounts\" WHERE \"Email\" = '{request.Email}'";
        var count = await _dbClient.TryGetIntAsync(query);
        return count > 0;
    }
}
