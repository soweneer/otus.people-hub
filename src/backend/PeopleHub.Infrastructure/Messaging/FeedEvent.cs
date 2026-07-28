using PeopleHub.Application.Models;

namespace PeopleHub.Infrastructure.Messaging;

public sealed record FeedEvent(FeedChangeType Type, FeedPost Post);
