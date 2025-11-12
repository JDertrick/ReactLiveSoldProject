using AutoMapper;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Base
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            // Map User
            CreateMap<UserProfileDto, User>();
            CreateMap<User, UserProfileDto>();
        }
    }
}
