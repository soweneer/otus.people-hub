using PeopleHub.Application.Models;

namespace PeopleHub.Model;

public sealed record FriendsViewModel(
    FriendsInfo FriendsInfo,
    IReadOnlyCollection<FeedPost> Feed);
