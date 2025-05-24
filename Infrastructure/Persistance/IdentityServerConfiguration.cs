using Duende.IdentityServer.Models;

namespace AuthGDPR.Infrastructure.Persistance
{
    public static class IdentityServerConfiguration
    {
        public static IEnumerable<Duende.IdentityServer.Models.IdentityResource> IdentityResources() =>
            new Duende.IdentityServer.Models.IdentityResource[]
                {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

        public static IEnumerable<ApiScope> ApiScopes() => new[]
        {
            new ApiScope("api1", "GDPR API"),
            new ApiScope("offline_access", "Refresh token access")
        };

        public static IEnumerable<Client> Clients() => new[]
        {
            // Swagger UI
            new Client
            {
                ClientId = "swagger-ui",
                ClientName = "Swagger UI for GDPR API",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { "https://https://localhost:5001/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "https://localhost:5001" },
                AllowedScopes = { "openid", "profile", "api1", "offline_access" },
                AllowOfflineAccess = true,

                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 1296000,
                AbsoluteRefreshTokenLifetime = 2592000
            },

            // Resource Owner Password
            new Client
            {
                ClientId = "ro.client",
                ClientName = "Resource Owner Password Client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedScopes = { "api1", "offline_access" },
                AllowOfflineAccess = true,

                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                AbsoluteRefreshTokenLifetime = 2592000
            }
        };
    }
}
