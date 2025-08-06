using AuthApi.Application.DTOs.ClientDtos;
using AuthApi.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace AuthApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly OpenIddictApplicationManager<MyApplication> _mgr;

        public ClientsController(OpenIddictApplicationManager<MyApplication> mgr)
        {
            _mgr = mgr;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAsync()
        {
            var applications = new List<object>();
            await foreach (var app in _mgr.ListAsync())
            {
                var cid = await _mgr.GetClientIdAsync(app);
                applications.Add(app);
            }
            return Ok(applications);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostAsync(ClientToCreateDto dto)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = dto.ClientId,
                ClientSecret = dto.ClientSecret,
                DisplayName = dto.DisplayName,
                Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                "permissions",
                "role",
                "offline_access",
                "email",
                "profile",
            }
            };

            var existingClientApp = await _mgr.FindByClientIdAsync(descriptor.ClientId!);
            if (existingClientApp != null)
                return Conflict($"Client '{dto.ClientId}' already exists."); 

            await _mgr.CreateAsync(descriptor);
            return Ok("Client added succussfully");
        }

        [HttpDelete("{clientId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest("Invalid clientId");

            var existing = await _mgr.FindByClientIdAsync(clientId);
            if (existing == null)
                return NotFound($"Client '{clientId}' not found");

            await _mgr.DeleteAsync(existing);
            return Ok("Client delete succussfully");
        }

    }
}
