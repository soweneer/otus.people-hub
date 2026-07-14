using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record LoginResponse(
    [property: JsonPropertyName("token")] string Token);
