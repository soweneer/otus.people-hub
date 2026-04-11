using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Person;
using PeopleHub.Lib.Model.Enums;

namespace PeopleHub.Lib.BusinessLogic.Person;

public sealed record GetRequest(int PersonId, int? CurrentPersonId = null): IRequest<DtoPerson>;

public sealed class GetHandler(DbClient dbClient) : IRequestHandler<GetRequest, DtoPerson>
{
    public async Task<DtoPerson> Handle(GetRequest request, CancellationToken cancellationToken)
    {
        var (personId, currentPersonId) = request;

        var query = currentPersonId.HasValue
            ? $"SELECT p.*, f.\"Status\" FROM \"{DbClient.PersonsTable}\" p LEFT JOIN \"{DbClient.FriendsTable}\" f ON " +
              $"(p.\"Id\" = f.\"SenderPersonId\" AND f.\"ReceiverPersonId\" = {currentPersonId.Value}) OR " +
              $"(p.\"Id\" = f.\"ReceiverPersonId\" AND f.\"SenderPersonId\" = {currentPersonId.Value}) " +
              $"WHERE p.\"Id\" = {personId}"
            : $"SELECT *, NULL::INTEGER AS \"Status\" FROM \"{DbClient.PersonsTable}\" WHERE \"Id\" = {personId}";
        var dataTable = await dbClient.GetDataTableAsync(query);
        if (dataTable.Rows.Count == 0)
            return null;

        var person = dataTable.Rows[0];
        return new DtoPerson
        {
            Id = Convert.ToInt32(person["Id"]),
            Surname = person["Surname"].ToString(),
            Name = person["Name"].ToString(),
            Age = Convert.ToInt32(person["Age"]),
            Bio = person["Bio"].ToString(),
            City = person["City"].ToString(),
            Gender = Enum.Parse<Gender>(person["Gender"].ToString()),
            Status = Convert.IsDBNull(person["Status"])
                ? FriendRequestStatus.None
                : Enum.Parse<FriendRequestStatus>(person["Status"].ToString())
        };
    }
}
