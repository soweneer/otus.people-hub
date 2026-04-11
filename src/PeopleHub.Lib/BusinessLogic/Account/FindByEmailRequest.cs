using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Account;

namespace PeopleHub.Lib.BusinessLogic.Account;

public sealed record FindByEmailRequest(string Email): IRequest<DtoAccount>;

public sealed class FindByEmailHandler(DbClient dbClient) : IRequestHandler<FindByEmailRequest, DtoAccount>
{
    public async Task<DtoAccount> Handle(FindByEmailRequest request, CancellationToken cancellationToken)
    {
        var dataTable = await dbClient.GetDataTableAsync($"SELECT * FROM \"Accounts\" WHERE \"Email\" = '{request.Email}'");
        return dataTable.Rows.Count == 0
            ? null
            : new DtoAccount
            {
                Email = dataTable.Rows[0]["Email"].ToString(),
                Password = dataTable.Rows[0]["Password"].ToString()
            };
    }
}
