using System.Data;

namespace PeopleHub.Domain.Entities;

public sealed record Post(
    long Id,
    string Text,
    int AuthorUserId)
{
    public static Post ExtractFromRow(DataRow row) =>
        new(
            Convert.ToInt64(row["id"]),
            row["text"].ToString(),
            Convert.ToInt32(row["author_user_id"])
        );
}
