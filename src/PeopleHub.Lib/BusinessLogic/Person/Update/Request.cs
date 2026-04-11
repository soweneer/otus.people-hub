using MediatR;
using PeopleHub.Lib.Model.Dto.Person;

namespace PeopleHub.Lib.BusinessLogic.Person.Update;

public sealed record Request(int PersonId, DtoUpdatePerson UpdateInfo): IRequest<DtoPerson>;