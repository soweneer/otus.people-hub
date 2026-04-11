using MediatR;
using PeopleHub.Lib.Model.Dto.Person;

namespace PeopleHub.Lib.BusinessLogic.Person.Get;

public sealed record Request(int PersonId, int? CurrentPersonId = null): IRequest<DtoPerson>;