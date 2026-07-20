do $$
declare
    extra_author_1 bigint;
    extra_author_2 bigint;
    reader_id bigint;
    author_id bigint;
    reader_names text[] := array['Denis','Marina','Timur','Alisa','Gleb','Sofia','Artem','Polina','Viktor'];
    reader_surnames text[] := array['Reader2','Reader3','Reader4','Reader5','Reader6','Reader7','Reader8','Reader9','Reader10'];
    base_authors bigint[] := array[1,3,4,5,6,7,8,9,10,11];
    i int;
    j int;
    pwd_hash text := 'Abu3iglrOi8DzwYiaYlb64xU0T+v8Fi/w7laFuusbdNcXXvKFkt7H4+sSeEU9ezaEw==';
begin
    if exists (select 1 from accounts where email = 'reader2-feed@test.com') then
        raise notice 'seed already applied, skipping';
        return;
    end if;

    insert into users (surname, name, age, gender, city, bio)
    values ('Pisarev', 'Igor', 31, 1, 'Москва', 'Пишу много постов')
    returning id into extra_author_1;

    insert into users (surname, name, age, gender, city, bio)
    values ('Blogerova', 'Vera', 27, 0, 'Казань', 'Блогер')
    returning id into extra_author_2;

    for j in 1..5 loop
        insert into posts (author_user_id, text, created_at)
        values (extra_author_1, 'Пост Игоря №' || j, now() - (random() * interval '7 days'));
    end loop;

    for j in 1..4 loop
        insert into posts (author_user_id, text, created_at)
        values (extra_author_2, 'Пост Веры №' || j, now() - (random() * interval '7 days'));
    end loop;

    for i in 1..9 loop
        insert into users (surname, name, age, gender, city, bio)
        values (reader_surnames[i], reader_names[i], 20 + i, i % 2, 'Москва', 'Читаю ленту')
        returning id into reader_id;

        insert into accounts (email, password, user_id)
        values ('reader' || (i + 1) || '-feed@test.com', pwd_hash, reader_id);

        foreach author_id in array base_authors loop
            insert into friend_requests (sender_user_id, receiver_user_id, status)
            values (reader_id, author_id, 2);
        end loop;

        if i <= 4 then
            insert into friend_requests (sender_user_id, receiver_user_id, status)
            values (reader_id, extra_author_1, 2);
            insert into friend_requests (sender_user_id, receiver_user_id, status)
            values (reader_id, extra_author_2, 2);
        elsif i <= 7 then
            insert into friend_requests (sender_user_id, receiver_user_id, status)
            values (reader_id, extra_author_1, 2);
        end if;
    end loop;
end $$;

select a.user_id, a.email, count(p.id) as feed_posts
from accounts a
join friend_requests fr
    on fr.status = 2 and (fr.sender_user_id = a.user_id or fr.receiver_user_id = a.user_id)
join posts p
    on p.author_user_id = case when fr.sender_user_id = a.user_id then fr.receiver_user_id else fr.sender_user_id end
where a.email like '%-feed@test.com' and a.email not like '%carol%'
group by a.user_id, a.email
union all
select a.user_id, a.email, count(p.id)
from accounts a
join friend_requests fr
    on fr.status = 2 and (fr.sender_user_id = a.user_id or fr.receiver_user_id = a.user_id)
join posts p
    on p.author_user_id = case when fr.sender_user_id = a.user_id then fr.receiver_user_id else fr.sender_user_id end
where a.email = 'carol-feed@test.com'
group by a.user_id, a.email
order by user_id;
