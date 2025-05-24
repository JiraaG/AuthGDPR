using Duende.IdentityServer.Models;

namespace AuthGDPR.Infrastructure
{
    public static class ConfigSeedData
    {
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client_app",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "api1" }
                    // Puoi aggiungere qui anche i RedirectUris, CORSOrigins, ecc.
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
                // Aggiungi altre risorse d’identità se necessario
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("api1", "Accesso a My API")
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API")
                {
                    Scopes = { "api1" }
                }
            };
        }

    }
}
