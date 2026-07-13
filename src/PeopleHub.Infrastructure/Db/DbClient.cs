using System.Data;
using Npgsql;

namespace PeopleHub.Infrastructure.Db;

// ReSharper disable once AsyncVoidLambda
internal sealed class DbClient(NpgsqlMultiHostDataSource dataSource)
{
    public const string UsersTable = "users";
    public const string FriendsRequestsTable = "friend_requests";
    public const string AccountsTable = "accounts";
    public const string PostsTable = "posts";

    private const string LegacyUsersTable = "persons";

    #region SQL для генерации тестовых данных

    private const string GenDataProcedureSql =
        $$"""
            create or replace procedure gen_data()
            language plpgsql
            as $$
            declare
                i integer;
                user_id_val integer;
                first_name_val text;
                last_name_val text;
                gender_val smallint;
                age_val smallint;
                city_val text;
                bio_val text;
                email_val text;
                random_receiver integer;
                request_status integer;
                first_names text[] := '{"Александр","Дмитрий","Максим","Сергей","Андрей","Алексей","Артём","Илья","Кирилл","Михаил","Никита","Даниил","Егор","Матвей","Роман","Владимир","Олег","Василий","Иван","Пётр","Анна","Мария","Елена","Ольга","Татьяна","Наталья","Ирина","Светлана","Екатерина","Юлия","Анастасия","Виктория","Дарья","Алёна","Полина","Валерия","Ксения","Вероника","Алина","Милана"}';
                last_names text[] := '{"Иванов","Смирнов","Кузнецов","Попов","Васильев","Петров","Соколов","Михайлов","Новиков","Фёдоров","Морозов","Волков","Алексеев","Лебедев","Семёнов","Егоров","Павлов","Козлов","Степанов","Николаев","Орлов","Андреев","Макаров","Никитин","Захаров","Кузнецова","Смирнова","Иванова","Попова","Васильева","Петрова","Соколова","Михайлова","Новикова","Фёдорова","Морозова","Волкова","Алексеева","Лебедева","Семёнова"}';
                cities text[] := '{"Москва","Санкт-Петербург","Новосибирск","Екатеринбург","Казань","Нижний Новгород","Челябинск","Самара","Омск","Ростов-на-Дону","Уфа","Красноярск","Пермь","Воронеж","Волгоград","Краснодар","Саратов","Тюмень","Тольятти","Ижевск"}';

                en_first_names text[] := '{"john","james","robert","michael","william","david","richard","thomas","charles","christopher","daniel","matthew","anthony","donald","mark","paul","steven","andrew","kenneth","joshua","kevin","brian","george","edward","ronald","mary","patricia","jennifer","linda","elizabeth","barbara","susan","jessica","sarah","karen","lisa","nancy","betty","margaret","sandra","ashley","kimberly","emily","donna","michelle","carol","amanda","melissa","deborah"}';
                en_last_names text[] := '{"smith","johnson","williams","brown","jones","garcia","miller","davis","rodriguez","martinez","hernandez","lopez","gonzalez","wilson","anderson","thomas","taylor","moore","jackson","martin","lee","white","harris","clark","lewis","robinson","walker","young","allen","king","wright","scott","torres","nguyen","hill","flores","green","adams","nelson","baker","hall","rivera","campbell","mitchell","carter","roberts"}';
            begin
                for i in 1..1000000 loop
                    gender_val := floor(random() * 2)::smallint;
                    first_name_val := first_names[1 + floor(random() * array_length(first_names, 1))];
                    last_name_val := last_names[1 + floor(random() * array_length(last_names, 1))];

                    if gender_val = 1 and last_name_val not like '%а' and last_name_val not like '%я' then
                        if right(last_name_val, 1) = 'в' or right(last_name_val, 1) = 'н' then
                            last_name_val := last_name_val || 'а';
                        elsif right(last_name_val, 1) = 'й' then
                            last_name_val := left(last_name_val, length(last_name_val)-1) || 'я';
                        end if;
                    end if;

                    age_val := (18 + floor(random() * 60))::smallint;
                    city_val := cities[1 + floor(random() * array_length(cities, 1))];

                    case floor(random() * 5)
                        when 0 then bio_val := 'Люблю путешествовать и открывать новое.';
                        when 1 then bio_val := 'IT-специалист, увлекаюсь спортом.';
                        when 2 then bio_val := 'Музыкант, играю на гитаре.';
                        when 3 then bio_val := 'Книги, кофе и долгие прогулки.';
                        else bio_val := null;
                    end case;

                    insert into {{UsersTable}} (surname, name, age, gender, city, bio)
                    values (last_name_val, first_name_val, age_val, gender_val, city_val, bio_val)
                    returning id into user_id_val;

                    -- declare
                    --     en_first text;
                    --     en_last text;
                    --     random_number integer;
                    -- begin
                    --     en_first := en_first_names[1 + floor(random() * array_length(en_first_names, 1))];
                    --     en_last := en_last_names[1 + floor(random() * array_length(en_last_names, 1))];
                    --     random_number := floor(random() * 9999);
                    --
                    --     -- разнообразие форматов email
                    --     case floor(random() * 6)
                    --         when 0 then email_val := lower(en_first || '.' || en_last || random_number || '@gmail.com');
                    --         when 1 then email_val := lower(en_first || en_last || random_number || '@yahoo.com');
                    --         when 2 then email_val := lower(en_first || '.' || en_last || '@outlook.com');
                    --         when 3 then email_val := lower(en_first || '_' || en_last || random_number || '@hotmail.com');
                    --         when 4 then email_val := lower(en_first || en_last || '@gmail.com');
                    --         else email_val := lower(en_first || '.' || en_last || random_number || '@icloud.com');
                    --     end case;
                    -- end;

                    -- insert into accounts (email, password, user_id)
                    -- values (email_val, 'AKqZ8b1KrUAH/UyHDedKvCRZsLeTk8qmAJuih9sxJNlMwfuLndQTH4SqnW/xExHotg==', user_id_val);

                    -- -- заявки в друзья (1-3 штуки)
                    -- for j in 1..(1 + floor(random() * 3)) loop
                    --     -- получаем случайного существующего пользователя
                    --     select id into random_receiver
                    --     from users
                    --     where id != user_id_val
                    --       and not exists (
                    --           select 1 from friend_requests
                    --           where sender_user_id = user_id_val and receiver_user_id = users.id
                    --       )
                    --     order by random()
                    --     limit 1;
                    --
                    --     if random_receiver is not null then
                    --         request_status := floor(random() * 3)::int;
                    --         insert into friend_requests (sender_user_id, receiver_user_id, status)
                    --         values (user_id_val, random_receiver, request_status);
                    --     end if;
                    -- end loop;

                    if i % 100 = 0 then
                        raise notice 'Создано % записей...', i;
                    end if;
                    commit;
                end loop;

                raise notice '! Готово !';
            end;
            $$;
        """;

    #endregion

    private async Task<NpgsqlConnection> GetSqlConnectionAsync(bool readOnly)
    {
        var connection = dataSource.CreateConnection(readOnly
            ? TargetSessionAttributes.Standby
            : TargetSessionAttributes.Primary);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<DataTable> GetDataTableAsync(string query, CancellationToken cancellationToken)
    {
        await using var connection = await GetSqlConnectionAsync(readOnly: true);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        var dataReader = await cmd.ExecuteReaderAsync(cancellationToken);
        var dataTable = new DataTable();
        dataTable.Load(dataReader);

        return dataTable;
    }

    public async Task<DataSet> GetDataSetASync(string query)
    {
        await using var connection = await GetSqlConnectionAsync(readOnly: true);
        using var dataAdapter = new NpgsqlDataAdapter(query, connection);
        var dataSet = new DataSet();
        dataAdapter.Fill(dataSet);
        return dataSet;
    }

    private async Task<List<string>> GetTableNamesAsync()
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

        return tableList;
    }

    private const string CreatePostsTableSql =
        $$"""
            create table if not exists {{PostsTable}} (
                id bigint generated always as identity primary key,
                author_user_id integer not null,
                text text not null,
                created_at timestamptz not null default now(),
                constraint posts_ibfk_1 foreign key (author_user_id) references {{UsersTable}} (id) on delete cascade
            );

            create index if not exists ix_{{PostsTable}}_author_user_id on {{PostsTable}} (author_user_id, id desc);
        """;

    private static bool TablesCreated(ICollection<string> tableNames) =>
        new[]
        {
            AccountsTable,
            FriendsRequestsTable,
            UsersTable
        }.All(tableNames.Contains);

    private async Task MigrateLegacySchemaAsync()
    {
        const string query =
            $$"""
                alter table {{LegacyUsersTable}} rename to {{UsersTable}};
                alter table {{AccountsTable}} rename column person_id to user_id;
                alter table {{FriendsRequestsTable}} rename column sender_person_id to sender_user_id;
                alter table {{FriendsRequestsTable}} rename column receiver_person_id to receiver_user_id;
                alter index if exists ix_{{LegacyUsersTable}}_surname_name rename to ix_{{UsersTable}}_surname_name;

                {{GenDataProcedureSql}}
            """;

        await ExecuteCmdAsync(query, async cmd => await cmd.ExecuteNonQueryAsync());
    }

    public async Task EnsureDbCreated()
    {
        var tableNames = await GetTableNamesAsync();
        if (tableNames.Contains(LegacyUsersTable))
        {
            await MigrateLegacySchemaAsync();
        }
        else if (!TablesCreated(tableNames))
        {
            await CreateBaseTablesAsync();
        }

        await ExecuteCmdAsync(CreatePostsTableSql, async cmd => await cmd.ExecuteNonQueryAsync());
    }

    private async Task CreateBaseTablesAsync()
    {
        const string query =
            #region SQL для создания базовых таблиц
            $$"""
                drop table if exists {{PostsTable}};
                drop table if exists {{FriendsRequestsTable}};
                drop table if exists {{AccountsTable}};
                drop table if exists {{UsersTable}};

                create table {{UsersTable}} (
                    id integer generated always as identity primary key,
                    surname varchar(100) not null,
                    name varchar(100) not null,
                    age smallint not null,
                    gender smallint not null,
                    city varchar(100) not null,
                    bio text
                );

                create table {{FriendsRequestsTable}} (
                    id integer generated always as identity primary key,
                    sender_user_id integer not null,
                    receiver_user_id integer not null,
                    status integer not null default 0,
                    constraint friends_ibfk_1 foreign key (sender_user_id) references {{UsersTable}} (id) on delete cascade,
                    constraint friends_ibfk_2 foreign key (receiver_user_id) references {{UsersTable}} (id) on delete cascade,
                    constraint friends_relation_unique unique (sender_user_id, receiver_user_id),
                    constraint friends_relation_unique_reverse unique (receiver_user_id, sender_user_id)
                );

                create table {{AccountsTable}} (
                    id integer generated always as identity primary key,
                    email varchar(100) not null,
                    password varchar(100) not null,
                    user_id integer not null,
                    constraint accounts_ibfk_1 foreign key (user_id) references {{UsersTable}} (id) on delete cascade
                );

                {{GenDataProcedureSql}}

                alter user postgres with password 'postgres';

                create extension pg_trgm;
                create index if not exists ix_{{UsersTable}}_surname_name
                    on {{UsersTable}} using gin (surname gin_trgm_ops, name gin_trgm_ops);
            """;
        #endregion

        await ExecuteCmdAsync(query, async cmd => await cmd.ExecuteNonQueryAsync());
    }

    public Task ExecuteNonQuery(string query, IEnumerable<(string, object)> parameters = null) =>
        ExecuteCmdAsync(query, async cmd => await cmd.ExecuteNonQueryAsync(), parameters);

    public async Task<object> ExecuteScalarAsync(string query, IEnumerable<(string, object)> parameters = null,
        bool readOnly = false)
    {
        var scalar = new object();
        await ExecuteCmdAsync(query, async cmd => scalar = await cmd.ExecuteScalarAsync(), parameters, readOnly);

        return scalar;
    }

    public async Task<DataTable> ExecuteDataTableAsync(string query, IEnumerable<(string, object)> parameters = null)
    {
        var dataTable = new DataTable();
        await ExecuteCmdAsync(query, async cmd =>
        {
            var dataReader = await cmd.ExecuteReaderAsync();
            dataTable.Load(dataReader);
        }, parameters, readOnly: true);

        return dataTable;
    }

    public async Task ExecuteCmdAsync(string parametrizedQuery, Func<NpgsqlCommand, Task> cmdAction,
        IEnumerable<(string, object)> parameters = null, bool readOnly = false)
    {
        await using var connection = await GetSqlConnectionAsync(readOnly);
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
