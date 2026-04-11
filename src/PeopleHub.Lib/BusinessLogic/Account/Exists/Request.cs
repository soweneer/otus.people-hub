using MediatR;

namespace PeopleHub.Lib.BusinessLogic.Account.Exists;

public sealed record Request(string Email): IRequest<bool>;