namespace PeopleHub.Chats.Domain;

public sealed class DialogMessage(Guid id, long fromUserId, long toUserId, string text)
{
    public Guid Id { get; } = id;
    public long FromUserId { get; } = fromUserId;
    public long ToUserId { get; } = toUserId;
    public string Text { get; } = text;

    public static DialogMessage Create(long fromUserId, long toUserId, string text) =>
        new(Guid.CreateVersion7(), fromUserId, toUserId, ValidateText(text));

    public static DialogMessage Restore(Guid id, long fromUserId, long toUserId, string text) =>
        new(id, fromUserId, toUserId, text);

    private static string ValidateText(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? throw new DomainException("Текст сообщения не может быть пустым")
            : text;
}
