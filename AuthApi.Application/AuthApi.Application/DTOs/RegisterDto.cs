using System.ComponentModel.DataAnnotations;

namespace AuthApi.Application.DTOs
{
    public record RegisterDto(
        [Required, EmailAddress] string Email,
        [Required] string FullName,
        [Required] string UserName,
        [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6), DataType(DataType.Password)] string Password
    );
}
