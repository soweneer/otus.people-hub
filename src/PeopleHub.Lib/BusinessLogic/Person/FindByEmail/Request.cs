using MediatR;

namespace PeopleHub.Lib.BusinessLogic.Person.FindByEmail;

public sealed record Request(string Email): IRequest<int>;