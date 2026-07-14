using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record RegisterUserRequest(
    [property: JsonPropertyName("first_name")] string FirstName,
    [property: JsonPropertyName("second_name")] string SecondName,
    [property: JsonPropertyName("age")] int? Age,
    [property: JsonPropertyName("biography")] string Biography,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("gender")] int? Gender);

public sealed record RegisterUserResponse(
    [property: JsonPropertyName("user_id")] string UserId);
