using PeopleHub.Application.Models;

namespace PeopleHub.Infrastructure.Caching;

public enum FeedChangeType
{
    Created = 0,
    Updated = 1,
    Deleted = 2
}

public sealed record FeedChangeEvent(FeedChangeType Type, FeedPost Post);
