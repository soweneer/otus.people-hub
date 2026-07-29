using nanoFramework.Tarantool.Model;
using PeopleHub.Chats.Db;

namespace PeopleHub.Chats.Tarantool;

internal sealed class TarantoolSchemaProbe(TarantoolConnectionPool connectionPool) : IDbMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await connectionPool.CallAsync("dialog_stats", TarantoolTuple.Create(), _ => true, cancellationToken);
    }
}
