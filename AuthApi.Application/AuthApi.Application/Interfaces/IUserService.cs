using AuthApi.Application.DTOs;

namespace AuthApi.Application.Interfaces
{
    public interface IUserService
    {
        Task<Response> Register(RegisterDto registerDto);
        Task<IEnumerable<GetUserDTO>> GetAllUsersAsync();
    }
}
