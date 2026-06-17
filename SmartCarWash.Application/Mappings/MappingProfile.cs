using AutoMapper;
using SmartCarWash.Application.DTOs;
using SmartCarWash.Domain.Entities;

namespace SmartCarWash.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            CreateMap<Tier, TierDto>().ReverseMap();
            CreateMap<CreateTierDto, Tier>();
            CreateMap<UpdateTierDto, Tier>();

            CreateMap<CustomerProfile, CustomerProfileDto>()
                .ForMember(dest => dest.CurrentTierName, opt => opt.MapFrom(src => src.CurrentTier != null ? src.CurrentTier.Name : null))
                .ReverseMap();
            CreateMap<CreateCustomerProfileDto, CustomerProfile>();
            CreateMap<UpdateCustomerProfileDto, CustomerProfile>();

            CreateMap<WashService, WashServiceDto>().ReverseMap();
            CreateMap<CreateWashServiceDto, WashService>();
            CreateMap<UpdateWashServiceDto, WashService>();

            CreateMap<Promotion, PromotionDto>().ReverseMap();
            CreateMap<CreatePromotionDto, Promotion>();
            CreateMap<UpdatePromotionDto, Promotion>();

            CreateMap<Vehicle, VehicleDto>().ReverseMap();
            CreateMap<CreateVehicleDto, Vehicle>();
            CreateMap<UpdateVehicleDto, Vehicle>();

            CreateMap<Booking, BookingDto>().ReverseMap();
            CreateMap<CreateBookingDto, Booking>();
            CreateMap<UpdateBookingDto, Booking>();

            CreateMap<PointLog, PointLogDto>().ReverseMap();
            CreateMap<CreatePointLogDto, PointLog>();
            CreateMap<UpdatePointLogDto, PointLog>();

            CreateMap<Feedback, FeedbackDto>().ReverseMap();
            CreateMap<CreateFeedbackDto, Feedback>();
            CreateMap<UpdateFeedbackDto, Feedback>();
        }
    }
}