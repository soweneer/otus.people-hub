namespace PeopleHub.Infrastructure.Db;

internal sealed class DbMigrator(DbClient dbClient) : IDbMigrator
{
    public Task MigrateAsync() => dbClient.EnsureDbCreated();
}
