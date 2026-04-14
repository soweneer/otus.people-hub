using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Model.Dto.Person;

namespace PeopleHub.Domain.BusinessLogic.Person;

public sealed record GetRequest(int PersonId, int? CurrentPersonId = null): IRequest<PersonDto>;

public sealed class GetHandler(DbClient dbClient) : IRequestHandler<GetRequest, PersonDto>
{
    public async Task<PersonDto> Handle(GetRequest request, CancellationToken cancellationToken)
    {
        var (personId, currentPersonId) = request;

        var query = currentPersonId.HasValue
            ? $"SELECT p.*, f.\"Status\" FROM \"{DbClient.PersonsTable}\" p LEFT JOIN \"{DbClient.FriendsTable}\" f ON " +
              $"(p.\"Id\" = f.\"SenderPersonId\" AND f.\"ReceiverPersonId\" = {currentPersonId.Value}) OR " +
              $"(p.\"Id\" = f.\"ReceiverPersonId\" AND f.\"SenderPersonId\" = {currentPersonId.Value}) " +
              $"WHERE p.\"Id\" = {personId}"
            : $"SELECT *, NULL::INTEGER AS \"Status\" FROM \"{DbClient.PersonsTable}\" WHERE \"Id\" = {personId}";
        var dataTable = await dbClient.GetDataTableAsync(query);

        return dataTable.Rows.Count == 0
            ? null
            : PersonDto.ExtractFromRow(dataTable.Rows[0]);
    }
}
