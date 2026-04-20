using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class AdminRepository(DbClient dbClient) : IAdminRepository
{
    public Task MigrateAsync() => dbClient.EnsureDbCreated();
}