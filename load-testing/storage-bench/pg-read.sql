\set i random(0, 39)
select id as dialog_id from dialogs where user_id1 = 2000000 + :i and user_id2 = 3000000 + :i \gset
select id, dialog_id, from_user_id, text from messages where dialog_id = :dialog_id order by id;
