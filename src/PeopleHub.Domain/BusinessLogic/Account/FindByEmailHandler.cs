using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Model.Dto.Account;

namespace PeopleHub.Domain.BusinessLogic.Account;

public sealed record FindByEmailRequest(string Email): IRequest<AccountDto>;

public sealed class FindByEmailHandler(DbClient dbClient) : IRequestHandler<FindByEmailRequest, AccountDto>
{
    public async Task<AccountDto> Handle(FindByEmailRequest request, CancellationToken cancellationToken)
    {
        var dataTable = await dbClient.GetDataTableAsync($"SELECT * FROM \"{DbClient.AccountsTable}\" WHERE \"email\" = '{request.Email}'");

        return dataTable.Rows.Count == 0
            ? null
            : new AccountDto(dataTable.Rows[0]["email"].ToString(), dataTable.Rows[0]["password"].ToString());
    }
}
