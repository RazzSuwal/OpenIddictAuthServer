using Microsoft.AspNetCore.Identity;

namespace AuthApi.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = null!;
    }
}
