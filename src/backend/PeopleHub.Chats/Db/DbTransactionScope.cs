using System.Data;
using Npgsql;

namespace PeopleHub.Chats.Db;

internal sealed class DbTransactionScope(NpgsqlConnection connection, NpgsqlTransaction transaction)
{
    public async Task<object> ExecuteScalarAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(query, parameters);

        return await command.ExecuteScalarAsync(cancellationToken);
    }

    public async Task ExecuteNonQueryAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(query, parameters);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<DataTable> ExecuteDataTableAsync(string query, IEnumerable<(string, object)> parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(query, parameters);

        var dataTable = new DataTable();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        dataTable.Load(reader);

        return dataTable;
    }

    private NpgsqlCommand CreateCommand(string query, IEnumerable<(string, object)> parameters)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;

        return command.Fill(query, parameters);
    }
}
