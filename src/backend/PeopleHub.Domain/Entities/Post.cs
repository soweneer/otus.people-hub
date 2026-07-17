using PeopleHub.Domain.Exceptions;

namespace PeopleHub.Domain.Entities;

public sealed class Post(long id, string text, int authorUserId)
{
    public long Id { get; } = id;
    public int AuthorUserId { get; } = authorUserId;
    public string Text { get; private set; } = text;

    public static Post Create(int authorUserId, string text) =>
        new(0, ValidateText(text), authorUserId);

    public static Post Restore(long id, int authorUserId, string text) =>
        new(id, text, authorUserId);

    public bool IsAuthoredBy(int userId) => userId == AuthorUserId;

    public void Edit(int editorUserId, string newText)
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
