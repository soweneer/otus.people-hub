using MediatR;
using PeopleHub.Dal.Infrastructure.Db;

namespace PeopleHub.Lib.BusinessLogic.Admin.MigrateDb;

public sealed class Handler : IRequestHandler<Request>
{
    private readonly DbClient _dbClient;

    public Handler(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    public Task Handle(Request request, CancellationToken cancellationToken)
    {
        return _dbClient.EnsureDbCreated();
    }
}
