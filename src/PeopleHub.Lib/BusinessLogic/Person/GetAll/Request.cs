using MediatR;
using PeopleHub.Lib.Model.Dto.Person;

namespace PeopleHub.Lib.BusinessLogic.Person.GetAll;

public sealed record Request(string PersonEmail): IRequest<IReadOnlyCollection<DtoPerson>>;