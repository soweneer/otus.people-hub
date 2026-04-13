using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Shared.Model.Dto.Account;

namespace PeopleHub.Shared.BusinessLogic.Account;

public sealed record FindByEmailRequest(string Email): IRequest<AccountDto>;

public sealed class FindByEmailHandler(DbClient dbClient) : IRequestHandler<FindByEmailRequest, AccountDto>
{
    public async Task<AccountDto> Handle(FindByEmailRequest request, CancellationToken cancellationToken)
    {
        var dataTable = await dbClient.GetDataTableAsync($"SELECT * FROM \"Accounts\" WHERE \"Email\" = '{request.Email}'");

        return dataTable.Rows.Count == 0
            ? null
            : new AccountDto(dataTable.Rows[0]["Email"].ToString(), dataTable.Rows[0]["Password"].ToString());
    }
}
