
using AutoMapper;
using DAL.Entities;
using DTO;
using System.Linq;

namespace BLL.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();

            CreateMap<HelpType, HelpTypeDto>();

            CreateMap<Volunteer, VolunteerDto>()
                .ForMember(
                    dest => dest.EquipmentTypeIds,
                    opt => opt.MapFrom(src =>
                        src.VolunteerEquipments
                            .Select(e => e.EquipmentTypeId)))
                .ForMember(
                    dest => dest.EquipmentTypeNames,
                    opt => opt.MapFrom(src =>
                        src.VolunteerEquipments
                            .Where(e => e.EquipmentType != null)
                            .Select(e => e.EquipmentType.Name)))
                .ForMember(
                    dest => dest.HelpTypeIds,
                    opt => opt.MapFrom(src =>
                        src.VolunteerHelpTypes
                            .Select(h => h.HelpTypeId)))
                .ForMember(
                    dest => dest.HelpTypeNames,
                    opt => opt.MapFrom(src =>
                        src.VolunteerHelpTypes
                            .Where(h => h.HelpType != null)
                            .Select(h => h.HelpType.Name)));

            CreateMap<HelpRequest, HelpRequestDto>()
                .ForMember(
                    dest => dest.RequiredEquipmentTypeIds,
                    opt => opt.MapFrom(src =>
                        src.RequiredEquipments.Select(e => e.EquipmentTypeId)))
                .ForMember(
                    dest => dest.RequiredEquipmentNames,
                    opt => opt.MapFrom(src =>
                        src.RequiredEquipments.Select(e => e.EquipmentType.Name)))
                .ReverseMap();

            CreateMap<HelpRequestPostDto, HelpRequest>()
                .ForMember(dest => dest.ImageName, opt => opt.Ignore());

            CreateMap<EquipmentType, EquipmentTypeDto>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<VolunteerEquipment, VolunteerEquipmentDto>()
                .ForMember(
                    dest => dest.EquipmentTypeName,
                    opt => opt.MapFrom(src =>
                        src.EquipmentType != null
                            ? src.EquipmentType.Name
                            : null));

            CreateMap<RequestEquipment, RequestEquipmentDto>()
                .ReverseMap();

            CreateMap<RequestOffer, RequestOfferDto>();
        }
    }
}
