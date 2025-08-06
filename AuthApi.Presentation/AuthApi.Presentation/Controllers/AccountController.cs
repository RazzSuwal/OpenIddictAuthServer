using AuthApi.Application.DTOs;
using AuthApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IUserService userService) : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<Response>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await userService.Register(registerDto);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpGet("GetAllUsers")]
        [Authorize]
        public async Task<ActionResult<GetUserDTO>> GetAllUsersAsync()
        {
            var user = await userService.GetAllUsersAsync();
            return user is not null ? Ok(user) : BadRequest(Request);
        }
    }
}

