using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record SendMessageRequest(
    [property: JsonPropertyName("text")] string Text);
