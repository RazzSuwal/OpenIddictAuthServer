namespace AuthApi.Application.DTOs
{
    public record GetUserDTO
    (
        Guid Id,
        string FullName,
        string UserName,
        string Email,
        IList<string> Roles
    );
}
