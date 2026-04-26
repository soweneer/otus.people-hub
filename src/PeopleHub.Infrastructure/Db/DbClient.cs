using System.Data;
using Npgsql;

namespace PeopleHub.Infrastructure.Db;

// ReSharper disable once AsyncVoidLambda
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
                drop table if exists {FriendsRequestsTable};
                drop table if exists {AccountsTable};
                drop table if exists {PersonsTable};
                
                create table {PersonsTable} (
                    id integer generated always as identity primary key,
                    surname varchar(100) not null,
                    name varchar(100) not null,
                    age smallint not null,
                    gender smallint not null,
                    city varchar(100) not null,
                    bio text
                );
                
                create table {FriendsRequestsTable} (
                    id integer generated always as identity primary key,
                    sender_person_id integer not null,
                    receiver_person_id integer not null,
                    status integer not null default 0,
                    constraint friends_ibfk_1 foreign key (sender_person_id) references {PersonsTable} (id) on delete cascade,
                    constraint friends_ibfk_2 foreign key (receiver_person_id) references {PersonsTable} (id) on delete cascade,
                    constraint friends_relation_unique unique (sender_person_id, receiver_person_id),
                    constraint friends_relation_unique_reverse unique (receiver_person_id, sender_person_id)
                );
                
                create table {AccountsTable} (
                    id integer generated always as identity primary key,
                    email varchar(100) not null,
                    password varchar(100) not null,
                    person_id integer not null,
                    constraint accounts_ibfk_1 foreign key (person_id) references {PersonsTable} (id) on delete cascade
                );
                
                create extension pg_trgm;
                create index if not exists ix_{PersonsTable}_surname_name
                    on {PersonsTable} using gin (surname gin_trgm_ops, name gin_trgm_ops);
            """;
        #endregion

        await ExecuteCmdAsync(query, async cmd => await cmd.ExecuteNonQueryAsync());
    }
    
    public Task ExecuteNonQuery(string query, IEnumerable<(string, object)> parameters = null) =>
        ExecuteCmdAsync(query, async cmd => await cmd.ExecuteNonQueryAsync(), parameters);

    public async Task<object> ExecuteScalarAsync(string query, IEnumerable<(string, object)> parameters = null)
    {
        var scalar = new object();
        await ExecuteCmdAsync(query, async cmd => scalar = await cmd.ExecuteScalarAsync(), parameters);

        return scalar;
    }
    
    public async Task<DataTable> ExecuteDataTableAsync(string query, IEnumerable<(string, object)> parameters = null)
    {
        var dataTable = new DataTable();
        await ExecuteCmdAsync(query, async cmd =>
        {
            var dataReader = await cmd.ExecuteReaderAsync();
            dataTable.Load(dataReader);
        }, parameters);
        
        return dataTable;
    }

    public async Task ExecuteCmdAsync(string parametrizedQuery, Func<NpgsqlCommand, Task> cmdAction,
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

        await cmdAction(cmd);

        await connection.CloseAsync();
    }
}
