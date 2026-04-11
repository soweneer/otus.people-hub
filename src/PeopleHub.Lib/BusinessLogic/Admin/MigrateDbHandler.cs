using MediatR;
using PeopleHub.Dal.Infrastructure.Db;

namespace PeopleHub.Lib.BusinessLogic.Admin;

public sealed record MigrateDbRequest: IRequest;

public sealed class MigrateDbHandler(DbClient dbClient) : IRequestHandler<MigrateDbRequest>
{
    public Task Handle(MigrateDbRequest request, CancellationToken cancellationToken)
    {
        return dbClient.EnsureDbCreated();
    }
}
