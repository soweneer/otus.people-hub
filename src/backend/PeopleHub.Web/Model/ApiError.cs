using System.Text.Json.Serialization;

namespace PeopleHub.Model;

public sealed record ApiError(
    [property: JsonPropertyName("error")] string Error);

public sealed record MeResponse(
    [property: JsonPropertyName("authenticated")] bool Authenticated,
    [property: JsonPropertyName("email")] string Email = null);

public sealed record SendFriendRequestBody(
    [property: JsonPropertyName("targetUserId")] int TargetUserId);
