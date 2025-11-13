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

            // Product Mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.ToString()))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TagLinks.Select(pt => pt.Tag)))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants));

            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TagLinks, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore());

            // ProductVariant Mappings
            CreateMap<ProductVariant, ProductVariantDto>();

            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Size, opt => opt.Ignore())
                .ForMember(dest => dest.Color, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrderItems, opt => opt.Ignore());

            // Tag Mappings
            CreateMap<Tag, TagDto>();
        }
    }
}
