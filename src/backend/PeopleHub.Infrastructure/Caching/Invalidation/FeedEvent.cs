using PeopleHub.Application.Models;

namespace PeopleHub.Infrastructure.Caching.Invalidation;

public sealed record FeedEvent(FeedChangeType Type, FeedPost Post);
