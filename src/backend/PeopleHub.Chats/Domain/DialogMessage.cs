namespace PeopleHub.Chats.Domain;

public sealed class DialogMessage(long id, long fromUserId, long toUserId, string text)
{
    public long Id { get; } = id;
    public long FromUserId { get; } = fromUserId;
    public long ToUserId { get; } = toUserId;
    public string Text { get; } = text;

    public static DialogMessage Create(long fromUserId, long toUserId, string text) =>
        new(0, fromUserId, toUserId, ValidateText(text));

    public static DialogMessage Restore(long id, long fromUserId, long toUserId, string text) =>
        new(id, fromUserId, toUserId, text);

    private static string ValidateText(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? throw new DomainException("Текст сообщения не может быть пустым")
            : text;
}
