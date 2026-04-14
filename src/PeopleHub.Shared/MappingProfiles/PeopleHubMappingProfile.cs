using AutoMapper;
using PeopleHub.Domain.Model.Dto.Person;
using PeopleHub.Shared.Model.View;

namespace PeopleHub.Shared.MappingProfiles
{
    public sealed class PeopleHubMappingProfile : Profile
    {
        public PeopleHubMappingProfile()
        {
            CreateMap<SignUpModel, PersonDto>();
            CreateMap<PersonDto, UpdatePersonDto>();
            CreateMap<UpdatePersonDto, PersonDto>();
        }
    }
}
