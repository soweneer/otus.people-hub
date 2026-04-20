using System.Data;
using Npgsql;

namespace PeopleHub.Infrastructure.Db;

internal sealed class DbClient(string connectionString)
{
    public const string PersonsTable = "persons";
    public const string FriendsRequestsTable = "friend_requests";
    public const string AccountsTable = "accounts";

    private async Task<NpgsqlConnection> GetSqlConnectionAsync()
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<DataTable> GetDataTableAsync(string query, CancellationToken cancellationToken)
    {
        await using var connection = await GetSqlConnectionAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        var dataReader = await cmd.ExecuteReaderAsync(cancellationToken);
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

    private async Task<bool> TablesCreated()
    {
        var tableList = new List<string>();

        await ExecuteCmdAsync("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'",
            // ReSharper disable once AsyncVoidLambda
            async cmd =>
            {
                var dataReader = await cmd.ExecuteReaderAsync();

                while (await dataReader.ReadAsync())
                {
                    tableList.Add(dataReader[0].ToString());
                }                
            });

        return tableList.OrderBy(t => t).SequenceEqual([
            AccountsTable,
            FriendsRequestsTable,
            PersonsTable
        ]);
    }

    public async Task EnsureDbCreated()
    {
        if (await TablesCreated())
            return;

        const string query =
            #region SQL для создания базовых таблиц
            $"""
                DROP TABLE IF EXISTS {FriendsRequestsTable};
                DROP TABLE IF EXISTS {AccountsTable};
                DROP TABLE IF EXISTS {PersonsTable};

                CREATE TABLE {PersonsTable} (
                    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    surname VARCHAR(100) NOT NULL,
                    name VARCHAR(100) NOT NULL,
                    age SMALLINT NOT NULL,
                    gender SMALLINT NOT NULL,
                    city VARCHAR(100) NOT NULL,
                    bio TEXT
                );

                CREATE TABLE {FriendsRequestsTable} (
                    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    sender_person_id INTEGER NOT NULL,
                    receiver_person_id INTEGER NOT NULL,
                    status INTEGER NOT NULL DEFAULT 0,
                    CONSTRAINT friends_ibfk_1 FOREIGN KEY (sender_person_id) REFERENCES {PersonsTable} (id) ON DELETE CASCADE,
                    CONSTRAINT friends_ibfk_2 FOREIGN KEY (receiver_person_id) REFERENCES {PersonsTable} (id) ON DELETE CASCADE,
                    CONSTRAINT friends_relation_unique UNIQUE (sender_person_id, receiver_person_id),
                    CONSTRAINT friends_relation_unique_reverse UNIQUE (receiver_person_id, sender_person_id)
                );

                CREATE TABLE {AccountsTable} (
                    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    email VARCHAR(100) NOT NULL,
                    password VARCHAR(100) NOT NULL,
                    person_id INTEGER NOT NULL,
                    CONSTRAINT accounts_ibfk_1 FOREIGN KEY (person_id) REFERENCES {PersonsTable} (id) ON DELETE CASCADE
                );
            """;
        
        // TODO uncomment me when got ready to do indexes homewwork
        // CREATE INDEX ix_{FriendsRequestsTable}_sender_person_id ON {FriendsRequestsTable} (sender_person_id);
        // CREATE INDEX ix_{FriendsRequestsTable}_receiver_person_id ON {FriendsRequestsTable} (receiver_person_id);
        // CREATE INDEX ix_accounts_person_id ON {AccountsTable} (person_id);
        #endregion

        await ExecuteCmdAsync(query, cmd => cmd.ExecuteNonQuery());
    }
    
    public Task ExecuteNonQuery(string query, IEnumerable<(string, object)> parameters = null) =>
        ExecuteCmdAsync(query, cmd => cmd.ExecuteNonQuery(), parameters);

    public async Task<object> ExecuteScalarAsync(string query, IEnumerable<(string, object)> parameters = null)
    {
        var scalar = new object();
        await ExecuteCmdAsync(query, cmd =>
        {
            scalar = cmd.ExecuteScalar();
        }, parameters);

        return scalar;
    }
    
    public async Task<DataTable> ExecuteDataTableAsync(string query, IEnumerable<(string, object)> parameters = null)
    {
        var dataTable = new DataTable();
        await ExecuteCmdAsync(query, cmd =>
        {
            var dataReader = cmd.ExecuteReader();
            dataTable.Load(dataReader);
        }, parameters);
        
        return dataTable;
    }

    public async Task ExecuteCmdAsync(string parametrizedQuery, Action<NpgsqlCommand> cmdAction, 
        IEnumerable<(string, object)> parameters = null)
    {
        await using var connection = await GetSqlConnectionAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = parametrizedQuery;

        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                cmd.Parameters.AddWithValue(parameter.Item1, parameter.Item2);    
            }
        }
        
        cmdAction.Invoke(cmd);
        
        await connection.CloseAsync();
    }
}
