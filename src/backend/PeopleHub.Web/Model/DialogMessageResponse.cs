using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record DialogMessageResponse(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("text")] string Text);
