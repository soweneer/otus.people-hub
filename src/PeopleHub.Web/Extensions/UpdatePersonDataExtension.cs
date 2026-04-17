using PeopleHub.Domain.Model;
using PeopleHub.Domain.Model.Dto.Person;

namespace PeopleHub.Extensions;

public static class UpdatePersonDataExtension
{
    extension(UpdatePersonRequest request)
    {
        public UpdatePersonData ToPersonData() => new(request.Name, 
            request.Surname,
            request.Age,
            request.City,
            request.Bio,
            request.Gender);
    }
}