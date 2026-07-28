\set i random(0, 199)
insert into dialogs (user_id1, user_id2) values (4200000 + :i, 5200000 + :i)
on conflict (user_id1, user_id2) do update set user_id1 = dialogs.user_id1
returning id as dialog_id \gset
insert into messages (dialog_id, from_user_id, text) values (:dialog_id, 4200000 + :i, 'storage bench');
