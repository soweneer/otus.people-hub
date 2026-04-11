using MediatR;
using PeopleHub.Lib.Model.Dto.Person;

namespace PeopleHub.Lib.BusinessLogic.Person.Create;

public sealed record Request(DtoPerson Person): IRequest<int?>;