using System.Data;
using Npgsql;

namespace PeopleHub.Infrastructure.Db;

public sealed class DbClient
{
    private readonly string _connectionString;
    public const string PersonsTable = "Persons";
    public const string FriendsTable = "FriendRequests";
    public const string AccountsTable = "Accounts";

    public DbClient(string connectionString)
    {
        _connectionString = connectionString;
    }

    private async Task<NpgsqlConnection> GetSqlConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<DataTable> GetDataTableAsync(string query)
    {
        await using var connection = await GetSqlConnectionAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        var dataReader = await cmd.ExecuteReaderAsync();
        var dataTable = new DataTable();
        dataTable.Load(dataReader);
        return dataTable;
    }

    public async Task<DataSet> GetDataSetASync(string query)
    {
        await using var connection = await GetSqlConnectionAsync();
        using var dataAdapter = new NpgsqlDataAdapter(query, connection);
        var dataSet = new DataSet();
        dataAdapter.Fill(dataSet);
        return dataSet;
    }

    public void RunCmd(string query)
    {
        using var connection = GetSqlConnectionAsync().GetAwaiter().GetResult();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        cmd.ExecuteNonQuery();
    }

    public async Task RunCmdAsync(string query)
    {
        await using var connection = await GetSqlConnectionAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<bool> TablesCreated()
    {
        var tableList = new List<string>();
        await using (var connection = await GetSqlConnectionAsync())
        await using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
            var dataReader = await cmd.ExecuteReaderAsync();

            while (await dataReader.ReadAsync())
                tableList.Add(dataReader[0].ToString().ToUpper());
        }

        return tableList.OrderBy(t => t).SequenceEqual(new[]
        {
            AccountsTable.ToUpper(),
            FriendsTable.ToUpper(),
            PersonsTable.ToUpper()
        });
    }

    public async Task EnsureDbCreated()
    {
        if (await TablesCreated())
            return;

        const string query =
            #region SQL для создания базовых таблиц
            $"""
                DROP TABLE IF EXISTS "{FriendsTable}";
                DROP TABLE IF EXISTS "{AccountsTable}";
                DROP TABLE IF EXISTS "{PersonsTable}";

                CREATE TABLE "{PersonsTable}" (
                  "Id" INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                  "Surname" VARCHAR(100) NOT NULL,
                  "Name" VARCHAR(100) NOT NULL,
                  "Age" SMALLINT NOT NULL,
                  "Gender" SMALLINT NOT NULL,
                  "City" VARCHAR(100) NOT NULL,
                  "Bio" TEXT
                );

                CREATE TABLE "{FriendsTable}" (
                  "Id" INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                  "SenderPersonId" INTEGER NOT NULL,
                  "ReceiverPersonId" INTEGER NOT NULL,
                  "Status" INTEGER NOT NULL DEFAULT 0,
                  CONSTRAINT "Friends_ibfk_1" FOREIGN KEY ("SenderPersonId") REFERENCES "{PersonsTable}" ("Id") ON DELETE CASCADE,
                  CONSTRAINT "Friends_ibfk_2" FOREIGN KEY ("ReceiverPersonId") REFERENCES "{PersonsTable}" ("Id") ON DELETE CASCADE,
                  CONSTRAINT "Friends_relation_unique" UNIQUE ("SenderPersonId", "ReceiverPersonId"),
                  CONSTRAINT "Friends_relation_unique_reverse" UNIQUE ("ReceiverPersonId", "SenderPersonId")
                );
                CREATE INDEX "IX_FriendRequests_SenderPersonId" ON "{FriendsTable}" ("SenderPersonId");
                CREATE INDEX "IX_FriendRequests_ReceiverPersonId" ON "{FriendsTable}" ("ReceiverPersonId");

                CREATE TABLE "{AccountsTable}" (
                  "Id" INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                  "Email" VARCHAR(100) NOT NULL,
                  "Password" VARCHAR(100) NOT NULL,
                  "PersonId" INTEGER NOT NULL,
                  CONSTRAINT "Accounts_ibfk_1" FOREIGN KEY ("PersonId") REFERENCES "{PersonsTable}" ("Id") ON DELETE CASCADE
                );
                CREATE INDEX "IX_Accounts_PersonId" ON "{AccountsTable}" ("PersonId");
            """;
        #endregion

        await RunCmdAsync(query);
    }

    public async Task<int?> TryGetIntAsync(string query)
    {
        await using var connection = await GetSqlConnectionAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        return int.TryParse(cmd.ExecuteScalar()?.ToString(), out var result)
            ? result
            : null;
    }
}
