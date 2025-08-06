using AuthApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;


namespace AuthApi.Infrastructure.Data
{
    public class MyApplication : OpenIddictEntityFrameworkCoreApplication<Guid, MyAuthorization, MyToken> { }
    public class MyAuthorization : OpenIddictEntityFrameworkCoreAuthorization<Guid, MyApplication, MyToken> { }
    public class MyScope : OpenIddictEntityFrameworkCoreScope<Guid> { }
    public class MyToken : OpenIddictEntityFrameworkCoreToken<Guid, MyApplication, MyAuthorization> { }
    
    public class AuthDbContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.UseOpenIddict<MyApplication, MyAuthorization, MyScope, MyToken, Guid>();

            builder.Entity<MyApplication>().ToTable("Applications");
            builder.Entity<MyAuthorization>().ToTable("Authorizations");
            builder.Entity<MyScope>().ToTable("Scopes");
            builder.Entity<MyToken>().ToTable("Tokens");

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        }
    }
}
