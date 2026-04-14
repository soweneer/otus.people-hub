using System.Data;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model.Dto.Person;

public sealed record PersonDto(int Id, string Name, string Surname, int Age, string City, Gender Gender,string Bio,
    FriendRequestStatus Status)
{
    // TODO move to extension
    public static PersonDto ExtractFromRow(DataRow row) =>
        new(
            Convert.ToInt32(row["Id"]),
            row["Name"].ToString(),
            row["Surname"].ToString(),
            Convert.ToInt32(row["Age"]),
            row["City"].ToString(),
            Enum.Parse<Gender>(row["Gender"].ToString()),
            row["Bio"].ToString(),
            Convert.IsDBNull(row["Status"])
                ? FriendRequestStatus.None
                : Enum.Parse<FriendRequestStatus>(row["Status"].ToString())
        );
}
