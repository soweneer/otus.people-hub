using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Account;

namespace PeopleHub.Lib.BusinessLogic.Account.FindByEmail;

public sealed class Handler : IRequestHandler<Request, DtoAccount>
{
    private readonly DbClient _dbClient;

    public Handler(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    public async Task<DtoAccount> Handle(Request request, CancellationToken cancellationToken)
    {
        var dataTable = await _dbClient.GetDataTableAsync($"SELECT * FROM \"Accounts\" WHERE \"Email\" = '{request.Email}'");
        return dataTable.Rows.Count == 0
            ? null
            : new DtoAccount
            {
                Email = dataTable.Rows[0]["Email"].ToString(),
                Password = dataTable.Rows[0]["Password"].ToString()
            };
    }
}
