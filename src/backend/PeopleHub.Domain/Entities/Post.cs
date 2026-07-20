using PeopleHub.Domain.Exceptions;

namespace PeopleHub.Domain.Entities;

public sealed class Post(long id, string text, long authorUserId)
{
    public long Id { get; } = id;
    public long AuthorUserId { get; } = authorUserId;
    public string Text { get; private set; } = text;

    public static Post Create(long authorUserId, string text) =>
        new(0, ValidateText(text), authorUserId);

    public static Post Restore(long id, int authorUserId, string text) =>
        new(id, text, authorUserId);

    public bool IsAuthoredBy(long userId) => userId == AuthorUserId;

    public void Edit(long editorUserId, string newText)
    {
        if (!IsAuthoredBy(editorUserId))
        {
            throw new DomainException($"Пост [{Id}] принадлежит другому пользователю");
        }

        Text = ValidateText(newText);
    }

    private static string ValidateText(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? throw new DomainException("Текст поста не может быть пустым")
            : text;
}
