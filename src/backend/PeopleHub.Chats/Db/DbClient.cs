using System.Data;
using Npgsql;

namespace PeopleHub.Chats.Db;

internal sealed class DbClient(NpgsqlDataSource dataSource)
{
    public const string DialogsTable = "dialogs";
    public const string MessagesTable = "messages";
    public const string DialogReadsTable = "dialog_reads";
    public const string OutboxTable = "outbox";

    public async Task<T> InTransactionAsync<T>(Func<DbTransactionScope, Task<T>> work,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var result = await work(new DbTransactionScope(connection, transaction));

        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    public async Task<object> ExecuteScalarAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = connection.CreateCommand().Fill(query, parameters);

        return await cmd.ExecuteScalarAsync(cancellationToken);
    }

    public async Task<DataTable> ExecuteDataTableAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = connection.CreateCommand().Fill(query, parameters);

        var dataTable = new DataTable();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        dataTable.Load(reader);

        return dataTable;
    }

    public async Task ExecuteNonQueryAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = connection.CreateCommand().Fill(query, parameters);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
