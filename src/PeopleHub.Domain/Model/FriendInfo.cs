using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model;

public sealed record FriendInfo(int Id, string Name, string Surname, int Age, string City, Gender Gender, string Bio, 
    FriendRequestStatus Status);