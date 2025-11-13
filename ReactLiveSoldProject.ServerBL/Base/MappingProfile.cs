using AutoMapper;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

namespace ReactLiveSoldProject.ServerBL.Base
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map User
            CreateMap<UserProfileDto, User>();
            CreateMap<User, UserProfileDto>();
            CreateMap<CreateUserDto, User>();

            // Map OrganizationMember to UserProfileDto
            // Este mapeo toma datos del User pero el Role del OrganizationMember
            CreateMap<OrganizationMember, UserProfileDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.OrganizationId))
                .ForMember(dest => dest.IsSuperAdmin, opt => opt.MapFrom(src => src.User.IsSuperAdmin));

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.ToString()));

            CreateMap<ProductDto, Product>();
        }
    }
}
