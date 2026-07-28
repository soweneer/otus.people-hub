local fiber = require('fiber')

local function bootstrap()
    local dialogs = box.schema.space.create('dialogs', { if_not_exists = true })
    dialogs:format({
        { name = 'id', type = 'unsigned' },
        { name = 'user_id1', type = 'unsigned' },
        { name = 'user_id2', type = 'unsigned' },
    })
    dialogs:create_index('primary', { parts = { 'id' }, if_not_exists = true })
    dialogs:create_index('pair', { parts = { 'user_id1', 'user_id2' }, unique = true, if_not_exists = true })
    dialogs:create_index('by_user2', { parts = { 'user_id2' }, unique = false, if_not_exists = true })

    local messages = box.schema.space.create('messages', { if_not_exists = true })
    messages:format({
        { name = 'dialog_id', type = 'unsigned' },
        { name = 'id', type = 'unsigned' },
        { name = 'from_user_id', type = 'unsigned' },
        { name = 'text', type = 'string' },
        { name = 'created_at', type = 'unsigned' },
    })
    messages:create_index('primary', { parts = { 'dialog_id', 'id' }, if_not_exists = true })

    box.schema.sequence.create('dialogs_seq', { if_not_exists = true })
    box.schema.sequence.create('messages_seq', { if_not_exists = true })
end

box.once('people_hub_dialogs_v1', bootstrap)

local function normalize(user_id1, user_id2)
    if user_id1 <= user_id2 then
        return user_id1, user_id2
    end

    return user_id2, user_id1
end

local function find_dialog(user_id1, user_id2)
    local left, right = normalize(user_id1, user_id2)

    return box.space.dialogs.index.pair:get({ left, right })
end

function dialog_send(from_user_id, to_user_id, text)
    if type(text) ~= 'string' or text:match('^%s*$') ~= nil then
        box.error({ code = 400, reason = 'Текст сообщения не может быть пустым' })
    end

    return box.atomic(function()
        local dialog = find_dialog(from_user_id, to_user_id)
        local dialog_id

        if dialog == nil then
            local left, right = normalize(from_user_id, to_user_id)
            dialog_id = box.sequence.dialogs_seq:next()
            box.space.dialogs:insert({ dialog_id, left, right })
        else
            dialog_id = dialog.id
        end

        local message_id = box.sequence.messages_seq:next()
        box.space.messages:insert({ dialog_id, message_id, from_user_id, text, math.floor(fiber.time()) })

        return message_id
    end)
end

function dialog_list(user_id1, user_id2)
    local messages = setmetatable({}, { __serialize = 'seq' })
    local dialog = find_dialog(user_id1, user_id2)

    if dialog == nil then
        return messages
    end

    for _, message in box.space.messages:pairs({ dialog.id }, { iterator = 'EQ' }) do
        table.insert(messages, { message.id, message.dialog_id, message.from_user_id, message.text })
    end

    return messages
end

function dialog_partners(user_id)
    local partners = setmetatable({}, { __serialize = 'seq' })

    for _, dialog in box.space.dialogs.index.pair:pairs({ user_id }, { iterator = 'EQ' }) do
        table.insert(partners, dialog.user_id2)
    end

    for _, dialog in box.space.dialogs.index.by_user2:pairs({ user_id }, { iterator = 'EQ' }) do
        table.insert(partners, dialog.user_id1)
    end

    table.sort(partners)

    return partners
end

function dialog_stats()
    return {
        dialogs = box.space.dialogs:len(),
        messages = box.space.messages:len(),
    }
end

local FUNCTIONS = { 'dialog_send', 'dialog_list', 'dialog_partners', 'dialog_stats' }

for _, name in ipairs(FUNCTIONS) do
    box.schema.func.create(name, { setuid = true, if_not_exists = true })
    box.schema.user.grant('guest', 'execute', 'function', name, { if_not_exists = true })
end
