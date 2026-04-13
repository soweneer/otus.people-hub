using MediatR;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Shared.BusinessLogic.Admin;

public sealed record MigrateDbRequest: IRequest;

public sealed class MigrateDbHandler(DbClient dbClient) : IRequestHandler<MigrateDbRequest>
{
    public Task Handle(MigrateDbRequest request, CancellationToken cancellationToken) => dbClient.EnsureDbCreated();
}
