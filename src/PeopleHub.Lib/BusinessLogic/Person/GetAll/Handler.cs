using System.Data;
using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Person;
using PeopleHub.Lib.Model.Enums;

namespace PeopleHub.Lib.BusinessLogic.Person.GetAll;

using FindPersonByEmailRequest = FindByEmail.Request;

public sealed class Handler : IRequestHandler<Request, IReadOnlyCollection<DtoPerson>>
{
    private readonly DbClient _dbClient;
    private readonly IMediator _mediator;

    public Handler(DbClient dbClient, IMediator mediator)
    {
        _dbClient = dbClient;
        _mediator = mediator;
    }

    public async Task<IReadOnlyCollection<DtoPerson>> Handle(Request request, CancellationToken cancellationToken)
    {
        var personId = await _mediator.Send(new FindPersonByEmailRequest(request.PersonEmail)
            , cancellationToken);

        await _dbClient.RunCmdAsync("DROP TABLE IF EXISTS \"MyFriends\"");
        var personList = new List<DtoPerson>();
        var createQuery = $"""
                CREATE TEMPORARY TABLE "MyFriends" AS
                SELECT DISTINCT "FriendId", "Status" FROM
                    (SELECT "SenderPersonId" AS "FriendId", "Status" FROM "{DbClient.FriendsTable}" WHERE "ReceiverPersonId" = {personId}
                    UNION ALL
                    SELECT "ReceiverPersonId" AS "FriendId", "Status" FROM "{DbClient.FriendsTable}" WHERE "SenderPersonId" = {personId}) AS TMP
            """;
        await _dbClient.RunCmdAsync(createQuery);
        var selectQuery = $"""
                SELECT p.*, f."Status"
                FROM
                    "Persons" p
                    LEFT JOIN "MyFriends" f ON f."FriendId" = p."Id"
                WHERE
                    p."Id" <> {personId}
            """;
        var dataTable = await _dbClient.GetDataTableAsync(selectQuery);
        if (dataTable == null || dataTable.Rows.Count == 0)
            return personList;
        personList.AddRange(
            from DataRow row in dataTable.Rows
            select new DtoPerson
            {
                Id = Convert.ToInt32(row["Id"]),
                Surname = row["Surname"].ToString(),
                Name = row["Name"].ToString(),
                Age = Convert.ToInt32(row["Age"]),
                Gender = Enum.Parse<Gender>(row["Gender"].ToString()),
                Bio = row["Bio"].ToString(),
                City = row["City"].ToString(),
                Status = Convert.IsDBNull(row["Status"])
                    ? FriendRequestStatus.None
                    : Enum.Parse<FriendRequestStatus>(row["Status"].ToString())
            });

        await _dbClient.RunCmdAsync("DROP TABLE IF EXISTS \"MyFriends\"");

        return personList;
    }
}
