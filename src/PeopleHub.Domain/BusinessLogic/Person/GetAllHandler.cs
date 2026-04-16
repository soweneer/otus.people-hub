using System.Data;
using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Model.Dto.Person;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.BusinessLogic.Person;

using FindPersonByEmailRequest = FindByEmailRequest;

public sealed record GetAllRequest(string PersonEmail): IRequest<IReadOnlyCollection<PersonDto>>;

public sealed class GetAllHandler(DbClient dbClient, IMediator mediator) : IRequestHandler<GetAllRequest, IReadOnlyCollection<PersonDto>>
{
    public async Task<IReadOnlyCollection<PersonDto>> Handle(GetAllRequest request, CancellationToken cancellationToken)
    {
        var personId = await mediator.Send(new FindPersonByEmailRequest(request.PersonEmail)
            , cancellationToken);

        await dbClient.RunCmdAsync("DROP TABLE IF EXISTS \"my_friends\"");
        var personList = new List<PersonDto>();
        var createQuery = $"""
                CREATE TEMPORARY TABLE "my_friends" AS
                SELECT DISTINCT "friend_id", "status" FROM
                    (SELECT "sender_person_id" AS "friend_id", "status" FROM "{DbClient.FriendsRequestsTable}" WHERE "receiver_person_id" = {personId}
                    UNION ALL
                    SELECT "receiver_person_id" AS "friend_id", "status" FROM "{DbClient.FriendsRequestsTable}" WHERE "sender_person_id" = {personId}) AS TMP
            """;
        await dbClient.RunCmdAsync(createQuery);
        var selectQuery = $"""
                SELECT p.*, f."status"
                FROM
                    "{DbClient.PersonsTable}" p
                    LEFT JOIN "my_friends" f ON f."friend_id" = p."id"
                WHERE
                    p."id" <> {personId}
            """;
        var dataTable = await dbClient.GetDataTableAsync(selectQuery);
        if (dataTable == null || dataTable.Rows.Count == 0)
            return personList;
        personList.AddRange(
            from DataRow row in dataTable.Rows
            select PersonDto.ExtractFromRow(row)
        );

        await dbClient.RunCmdAsync("DROP TABLE IF EXISTS \"my_friends\"");

        return personList;
    }
}
