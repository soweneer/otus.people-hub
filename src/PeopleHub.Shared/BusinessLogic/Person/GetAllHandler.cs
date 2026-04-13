using System.Data;
using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Shared.Model.Dto.Person;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Shared.BusinessLogic.Person;

using FindPersonByEmailRequest = FindByEmailRequest;

public sealed record GetAllRequest(string PersonEmail): IRequest<IReadOnlyCollection<PersonDto>>;

public sealed class GetAllHandler(DbClient dbClient, IMediator mediator) : IRequestHandler<GetAllRequest, IReadOnlyCollection<PersonDto>>
{

    public async Task<IReadOnlyCollection<PersonDto>> Handle(GetAllRequest request, CancellationToken cancellationToken)
    {
        var personId = await mediator.Send(new FindPersonByEmailRequest(request.PersonEmail)
            , cancellationToken);

        await dbClient.RunCmdAsync("DROP TABLE IF EXISTS \"MyFriends\"");
        var personList = new List<PersonDto>();
        var createQuery = $"""
                CREATE TEMPORARY TABLE "MyFriends" AS
                SELECT DISTINCT "FriendId", "Status" FROM
                    (SELECT "SenderPersonId" AS "FriendId", "Status" FROM "{DbClient.FriendsTable}" WHERE "ReceiverPersonId" = {personId}
                    UNION ALL
                    SELECT "ReceiverPersonId" AS "FriendId", "Status" FROM "{DbClient.FriendsTable}" WHERE "SenderPersonId" = {personId}) AS TMP
            """;
        await dbClient.RunCmdAsync(createQuery);
        var selectQuery = $"""
                SELECT p.*, f."Status"
                FROM
                    "Persons" p
                    LEFT JOIN "MyFriends" f ON f."FriendId" = p."Id"
                WHERE
                    p."Id" <> {personId}
            """;
        var dataTable = await dbClient.GetDataTableAsync(selectQuery);
        if (dataTable == null || dataTable.Rows.Count == 0)
            return personList;
        personList.AddRange(
            from DataRow row in dataTable.Rows
            select PersonDto.ExtractFromRow(row)
        );

        await dbClient.RunCmdAsync("DROP TABLE IF EXISTS \"MyFriends\"");

        return personList;
    }
}
