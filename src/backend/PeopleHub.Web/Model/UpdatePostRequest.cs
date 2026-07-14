using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record UpdatePostRequest(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("text")] string Text);
