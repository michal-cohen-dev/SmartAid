using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DAL.Entities;
using DTO;

namespace BLL.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<Volunteer, VolunteerDto>()
     .ReverseMap()
     .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<HelpRequest, HelpRequestDto>().ReverseMap();

            CreateMap<HelpRequestPostDto, HelpRequest>()
                .ForMember(dest => dest.ImageName, opt => opt.Ignore());

            CreateMap<EquipmentType, EquipmentTypeDto>()
          .ReverseMap()
          .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<VolunteerEquipment, VolunteerEquipmentDto>().ReverseMap();
            CreateMap<RequestEquipment, RequestEquipmentDto>().ReverseMap();
            CreateMap<RequestOffer, RequestOfferDto>().ReverseMap();
        }
    }
}
