using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IUserService
    {
        Task<ICollection<UserProfileDto>> GetUserAsync(Guid organizationId);
        Task<UserProfileDto> CreateUserAsync(CreateUserDto dto);
    }
}
