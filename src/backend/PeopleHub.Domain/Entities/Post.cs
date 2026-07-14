using PeopleHub.Domain.Exceptions;

namespace PeopleHub.Domain.Entities;

public sealed class Post
{
    private Post(long id, int authorUserId, string text)
    {
        Id = id;
        AuthorUserId = authorUserId;
        Text = text;
    }

    public long Id { get; }
    public int AuthorUserId { get; }
    public string Text { get; private set; }

    public static Post Create(int authorUserId, string text) =>
        new(0, authorUserId, ValidateText(text));

    public static Post Restore(long id, int authorUserId, string text) =>
        new(id, authorUserId, text);

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
