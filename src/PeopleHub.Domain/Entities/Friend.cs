using System.Data;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Entities;

public sealed record Friend(
    int Id,
    string Name,
    string Surname,
    int Age, 
    string City,
    Gender Gender,
    string Bio,
    FriendRequestStatus Status)
{
    public static Friend ExtractFromRow(DataRow row) =>
        new(
            Convert.ToInt32(row["id"]),
            row["name"].ToString(),
            row["surname"].ToString(),
            Convert.ToInt32(row["age"]),
            row["city"].ToString(),
            Enum.Parse<Gender>(row["gender"].ToString()),
            row["bio"].ToString(),
            Convert.IsDBNull(row["status"])
                ? FriendRequestStatus.None
                : Enum.Parse<FriendRequestStatus>(row["status"].ToString())
        );
}