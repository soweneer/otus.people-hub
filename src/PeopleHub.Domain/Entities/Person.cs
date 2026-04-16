using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Entities;

public sealed record Person(
    int Id,
    string Name,
    string Surname,
    int Age, 
    string City,
    Gender Gender,
    string Bio,
    FriendRequestStatus Status);