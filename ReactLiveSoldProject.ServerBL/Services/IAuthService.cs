using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> EmployeeLoginAsync(LoginRequestDto request);
        Task<LoginResponseDto> CustomerPortalLoginAsync(CustomerPortalLoginRequestDto request);
        Task<UserProfileDto> GetEmployeeProfileAsync(Guid userId);
        Task<CustomerProfileDto> GetCustomerProfileAsync(Guid customerId);
    }
}
