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
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Volunteer, VolunteerDto>().ReverseMap();
            CreateMap<HelpRequest, HelpRequestDto>().ReverseMap();
            CreateMap<EquipmentType, EquipmentTypeDto>().ReverseMap();
            CreateMap<VolunteerEquipment, VolunteerEquipmentDto>().ReverseMap();
            CreateMap<RequestEquipment, RequestEquipmentDto>().ReverseMap();
            CreateMap<RequestOffer, RequestOfferDto>().ReverseMap();
        }
    }
}
