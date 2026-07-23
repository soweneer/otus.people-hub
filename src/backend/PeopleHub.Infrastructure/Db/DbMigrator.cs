namespace PeopleHub.Infrastructure.Db;

internal sealed class DbMigrator(DbClient dbClient) : IDbMigrator
{
    private const string SeedSql =
    """
    do $$
    declare
        carol_id bigint;
        author_ids bigint[] := '{}';
        reader_ids bigint[] := '{}';
        new_id bigint;
        author_id bigint;
        reader_id bigint;
        i int;
        j int;
        pwd_hash text := 'Abu3iglrOi8DzwYiaYlb64xU0T+v8Fi/w7laFuusbdNcXXvKFkt7H4+sSeEU9ezaEw==';
        reader_emails text[] := array[
            'reader2-feed@test.com','reader3-feed@test.com','reader4-feed@test.com',
            'reader5-feed@test.com','reader6-feed@test.com','reader7-feed@test.com',
            'reader8-feed@test.com','reader9-feed@test.com','reader10-feed@test.com'
        ];
    begin
        if exists (select 1 from accounts where email = 'carol-feed@test.com') then
            raise notice 'feed seed already applied, skipping';
            return;
        end if;

        insert into users (surname, name, age, gender, city, bio)
        values ('Author', 'One', 30, 1, 'Москва', 'Автор постов')
        returning id into new_id;
        author_ids := array_append(author_ids, new_id);

        insert into users (surname, name, age, gender, city, bio)
        values ('Carol', 'Reader', 28, 0, 'Москва', 'Читаю ленту')
        returning id into carol_id;

        for i in 1..11 loop
            insert into users (surname, name, age, gender, city, bio)
            values ('Author', 'N' || (i + 1), 25 + i, i % 2, 'Москва', 'Автор постов')
            returning id into new_id;
            author_ids := array_append(author_ids, new_id);
        end loop;

        for i in 1..9 loop
            insert into users (surname, name, age, gender, city, bio)
            values ('Reader', 'N' || (i + 1), 20 + i, i % 2, 'Москва', 'Читаю ленту')
            returning id into new_id;
            reader_ids := array_append(reader_ids, new_id);
        end loop;

        foreach author_id in array author_ids loop
            for j in 1..3 loop
                insert into posts (author_user_id, text, created_at)
                values (author_id, 'Пост автора ' || author_id || ' №' || j, now() - (random() * interval '7 days'));
            end loop;
        end loop;

        insert into accounts (email, password, user_id) values ('carol-feed@test.com', pwd_hash, carol_id);
        for i in 1..9 loop
            insert into accounts (email, password, user_id) values (reader_emails[i], pwd_hash, reader_ids[i]);
        end loop;

        foreach author_id in array author_ids loop
            insert into friend_requests (sender_user_id, receiver_user_id, status) values (carol_id, author_id, 2);
            foreach reader_id in array reader_ids loop
                insert into friend_requests (sender_user_id, receiver_user_id, status) values (reader_id, author_id, 2);
            end loop;
        end loop;
    end $$;
    """;

    private Task SeedAsync() => dbClient.ExecuteNonQuery(SeedSql);
    
    public async Task MigrateAsync()
    {
        await dbClient.EnsureDbCreated();
        await SeedAsync();
    }
}
