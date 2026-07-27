using System.Text.Json.Serialization;

namespace PeopleHub.Infrastructure.Messaging;

public sealed record FeedPostedNotification(
    [property: JsonPropertyName("postId")] string PostId,
    [property: JsonPropertyName("postText")] string PostText,
    [property: JsonPropertyName("author_user_id")] string AuthorUserId);
