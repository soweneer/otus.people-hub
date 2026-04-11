using AutoMapper;
using PeopleHub.Lib.Model.Dto.Person;
using PeopleHub.Lib.Model.View;

namespace PeopleHub.Lib.MappingProfiles
{
    public sealed class PeopleHubMappingProfile : Profile
    {
        public PeopleHubMappingProfile()
        {
            CreateMap<SignUpModel, DtoPerson>();
//                .ForMember(d => d.Surname, o => o.MapFrom(s => s.Surname))
//                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
//                .ForMember(d => d.Age, o => o.MapFrom(s => s.Age))
//                .ForMember(d => d.City, o => o.MapFrom(s => s.City))
//                .ForMember(d => d.Gender,o => o.MapFrom(s => Enum.Parse<Gender>(s.Gender.ToString())))
//                .ForMember(d => d.Bio, o => o.MapFrom(s => s.Bio));
            CreateMap<DtoPerson, DtoUpdatePerson>();
            CreateMap<DtoUpdatePerson, DtoPerson>();
        }
    }
}