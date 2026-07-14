using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record LoginRequest(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("password")] string Password);
