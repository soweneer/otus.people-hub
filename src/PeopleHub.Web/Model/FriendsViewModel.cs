using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;

namespace PeopleHub.Model;

public sealed record FriendsViewModel(
    FriendsInfo FriendsInfo,
    IReadOnlyCollection<Post> Feed);
