using MediatR;
using PeopleHub.Lib.Model.Dto.Account;

namespace PeopleHub.Lib.BusinessLogic.Account.FindByEmail;

public sealed record Request(string Email): IRequest<DtoAccount>;