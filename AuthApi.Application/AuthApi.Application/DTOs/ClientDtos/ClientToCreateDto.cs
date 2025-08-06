using System.ComponentModel.DataAnnotations;

namespace AuthApi.Application.DTOs.ClientDtos
{
    public record ClientToCreateDto
    (
        [Required] string ClientId,
        [Required] string ClientSecret,
        [Required] string DisplayName,
        [Url] string RedirectUri
    );
}



