using System.Data;
using Npgsql;

namespace PeopleHub.Chats.Db;

internal sealed class DbClient(NpgsqlDataSource dataSource)
{
    public const string DialogsTable = "dialogs";

    public async Task<object> ExecuteScalarAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = connection.CreateCommand();
        FillCommand(cmd, query, parameters);

        return await cmd.ExecuteScalarAsync(cancellationToken);
    }

    public async Task<DataTable> ExecuteDataTableAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = connection.CreateCommand();
        FillCommand(cmd, query, parameters);

        var dataTable = new DataTable();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        dataTable.Load(reader);

        return dataTable;
    }

    public async Task ExecuteNonQueryAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = connection.CreateCommand();
        FillCommand(cmd, query, parameters);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void FillCommand(NpgsqlCommand cmd, string query, IEnumerable<(string, object)> parameters)
    {
        cmd.CommandText = query;

        if (parameters is null)
        {
            return;
        }

        foreach (var (name, value) in parameters)
        {
            cmd.Parameters.AddWithValue(name, value);
        }
    }
}
