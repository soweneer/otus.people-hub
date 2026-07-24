namespace PeopleHub.Chats.Domain;

public sealed class DialogMessage(long id, long dialogId, long fromUserId, string text)
{
    public long Id { get; } = id;
    public long DialogId { get; } = dialogId;
    public long FromUserId { get; } = fromUserId;
    public string Text { get; } = text;

    public static DialogMessage Create(long dialogId, long fromUserId, string text) =>
        new(0, dialogId, fromUserId, ValidateText(text));

    public static DialogMessage Restore(long id, long dialogId, long fromUserId, string text) =>
        new(id, dialogId, fromUserId, text);

    private static string ValidateText(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? throw new DomainException("Текст сообщения не может быть пустым")
            : text;
}
