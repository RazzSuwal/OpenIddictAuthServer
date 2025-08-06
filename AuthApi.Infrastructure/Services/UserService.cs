using AuthApi.Application.DTOs;
using AuthApi.Application.Interfaces;
using AuthApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AuthApi.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        private async Task<ApplicationUser> GetUserByEmail(string email)
        {
            var normalized = _userManager.NormalizeEmail(email.Trim());

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized);

            return user is null ? null! : user;
        }
        private async Task<ApplicationUser> GetUserByUserName(string userName)
        {
            var normalized = _userManager.NormalizeName(userName.Trim());
            var user = await _userManager.Users
               .FirstOrDefaultAsync(u => u.NormalizedUserName == normalized);
            return user is null ? null! : user;
        }
        public async Task<Response> Register(RegisterDto registerDto)
        {
            var getUserName = await GetUserByUserName(registerDto.UserName);
            if (getUserName is not null)
                return new Response(false, $"UserName already taken");

            var getUserEmail = await GetUserByEmail(registerDto.Email);
            if (getUserEmail is not null)
                return new Response(false, $"Email already taken");

            var user = new ApplicationUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FullName = registerDto.FullName
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            return result.Succeeded ? new Response(true, "User registered successfully") :
            new Response(false, "Invalid data provided");
        }

        public async Task<IEnumerable<GetUserDTO>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<GetUserDTO>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userDtos.Add(new GetUserDTO(
                    u.Id,
                    u.FullName!,
                    u.UserName!,
                    u.Email!,
                    roles  
                ));
            }

            return userDtos;
        }
    }
}
