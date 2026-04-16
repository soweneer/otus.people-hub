using MediatR;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Domain.BusinessLogic.Account;

public sealed record CreateRequest(int PersonId, string Email, string Password): IRequest<int?>;

public sealed class CreateHandler(DbClient dbClient) : IRequestHandler<CreateRequest, int?>
{
    public async Task<int?> Handle(CreateRequest request, CancellationToken cancellationToken)
    {
        return await dbClient.TryGetIntAsync(
            $"INSERT INTO \"{DbClient.AccountsTable}\" (\"email\", \"password\", \"personId\") " +
            $"VALUES ('{request.Email}', '{request.Password}', {request.PersonId}) RETURNING \"Id\"");
    }
}
