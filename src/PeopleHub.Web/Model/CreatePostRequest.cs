using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record CreatePostRequest(
    [property: JsonPropertyName("text")] string Text);
