using AutoMapper;
using PeopleHub.Shared.Model.Dto.Person;
using PeopleHub.Shared.Model.View;

namespace PeopleHub.Shared.MappingProfiles
{
    public sealed class PeopleHubMappingProfile : Profile
    {
        public PeopleHubMappingProfile()
        {
            CreateMap<SignUpModel, PersonDto>();
//                .ForMember(d => d.Surname, o => o.MapFrom(s => s.Surname))
//                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
//                .ForMember(d => d.Age, o => o.MapFrom(s => s.Age))
//                .ForMember(d => d.City, o => o.MapFrom(s => s.City))
//                .ForMember(d => d.Gender,o => o.MapFrom(s => Enum.Parse<Gender>(s.Gender.ToString())))
//                .ForMember(d => d.Bio, o => o.MapFrom(s => s.Bio));
            CreateMap<PersonDto, UpdatePersonDto>();
            CreateMap<UpdatePersonDto, PersonDto>();
        }
    }
}
