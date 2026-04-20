using PeopleHub.Domain.Model;
using PeopleHub.Model;

namespace PeopleHub.Extensions;

public static class UpdateMyProfileRequestExtension
{
    extension(UpdateMyProfileRequest request)
    {
        public PersonalInfo ExtractPersonalInfo() => new(request.Name, 
            request.Surname,
            request.Age,
            request.City,
            request.Bio,
            request.Gender);
    }
}