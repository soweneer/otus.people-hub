using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record PostResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("author_user_id")] string AuthorUserId);
