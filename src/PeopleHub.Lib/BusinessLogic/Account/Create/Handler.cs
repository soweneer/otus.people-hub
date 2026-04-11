using MediatR;
using PeopleHub.Dal.Infrastructure.Db;

namespace PeopleHub.Lib.BusinessLogic.Account.Create;

public sealed class Handler : IRequestHandler<Request, int?>
{
    private readonly DbClient _dbClient;

    public Handler(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    public async Task<int?> Handle(Request request, CancellationToken cancellationToken)
    {
        return await _dbClient.TryGetIntAsync(
            "INSERT INTO \"Accounts\" (\"Email\", \"Password\", \"PersonId\") " +
            $"VALUES ('{request.Email}', '{request.Password}', {request.PersonId}) RETURNING \"Id\"");
    }
}
