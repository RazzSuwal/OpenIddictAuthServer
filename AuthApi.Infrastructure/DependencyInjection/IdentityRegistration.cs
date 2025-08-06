using AuthApi.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthApi.Infrastructure.DependencyInjection
{
    public static class IdentityRegistration
    {
        public static WebApplicationBuilder RegisterIdentityClient(this WebApplicationBuilder builder)
        {
            builder.Services.AddHostedService<IdentityClientRegistrationService>();
            return builder;
        }
    }

    public class IdentityClientRegistrationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public IdentityClientRegistrationService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            await context.Database.EnsureCreatedAsync();

            var authority = _configuration.GetValue<string>("Auth:Authority");
            var clientId = _configuration.GetValue<string>("Auth:ClientId");
            var clientSecret = _configuration.GetValue<string>("Auth:ClientSecret");

            ArgumentException.ThrowIfNullOrEmpty(authority, nameof(authority));
            ArgumentException.ThrowIfNullOrEmpty(clientId, nameof(clientId));
            ArgumentException.ThrowIfNullOrEmpty(clientSecret, nameof(clientSecret));

            var existingClient = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
            if (existingClient == null)
            {
                var client = new OpenIddictApplicationDescriptor
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    DisplayName = clientId,
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,
                        "permissions",
                        "role",
                        "offline_access",
                        "email",
                        "profile",
                    }
                };
                await applicationManager.CreateAsync(client, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}