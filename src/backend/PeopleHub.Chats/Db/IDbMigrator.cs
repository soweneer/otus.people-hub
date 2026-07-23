namespace PeopleHub.Chats.Db;

public interface IDbMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
