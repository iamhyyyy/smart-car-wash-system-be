using AutoMapper;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Vehicle, VehicleDto>().ReverseMap();
        }
    }
}
