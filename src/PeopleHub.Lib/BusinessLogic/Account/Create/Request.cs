using MediatR;

namespace PeopleHub.Lib.BusinessLogic.Account.Create;

public sealed record Request(int PersonId, string Email, string Password): IRequest<int?>;