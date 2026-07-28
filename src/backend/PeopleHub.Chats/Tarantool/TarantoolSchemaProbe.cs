using PeopleHub.Chats.Db;

namespace PeopleHub.Chats.Tarantool;

internal sealed class TarantoolSchemaProbe(TarantoolClient client) : IDbMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await client.CallAsync("dialog_stats", [], _ => true, cancellationToken);
    }
}
