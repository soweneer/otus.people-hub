using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record UserResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("first_name")] string FirstName,
    [property: JsonPropertyName("second_name")] string SecondName,
    [property: JsonPropertyName("biography")] string Biography,
    [property: JsonPropertyName("city")] string City);
