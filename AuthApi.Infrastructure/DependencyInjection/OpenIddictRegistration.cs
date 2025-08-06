using AuthApi.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AuthApi.Infrastructure.DependencyInjection
{
    public static class OpenIddictRegistration
    {
        public static WebApplicationBuilder OpenIddictServerRegistration(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                    .UseDbContext<AuthDbContext>()
                    .ReplaceDefaultEntities<MyApplication, MyAuthorization, MyScope, MyToken, Guid>();
                })
                .AddServer(options =>
                {
                    options
                    .SetAuthorizationEndpointUris("connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetEndSessionEndpointUris("/connect/logout");

                    //options.AllowClientCredentialsFlow();
                    options.AllowAuthorizationCodeFlow()
                           .AllowPasswordFlow()
                           .AllowRefreshTokenFlow();

                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    options.UseAspNetCore()
                           .EnableEndSessionEndpointPassthrough()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableTokenEndpointPassthrough();
                })

                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            return builder;
        }
    }
}
