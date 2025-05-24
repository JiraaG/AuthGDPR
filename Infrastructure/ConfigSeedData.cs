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
                    ClientId = "swagger-ui",
                    ClientName = "Swagger UI",
                    AllowedGrantTypes = GrantTypes.Code, // Abilita il flow Authorization Code (adatto a Swagger UI e applicazioni web)
                    RequirePkce = true,                  // Richiede PKCE per maggiore sicurezza (consigliato per client pubblici)
                    RequireClientSecret = false,         // Se false, Swagger UI NON richiede il client_secret (tipico per test locali e JS)

                    // AllowedGrantTypes = GrantTypes.ClientCredentials
                    // RequireClientSecret = true,       // Se vuoi richiedere il client_secret, imposta true e decommenta la riga sotto
                    // ClientSecrets = { new Secret("swagger-secret".Sha256()) }, // Definisci qui il client_secret (inserisci la stringa in chiaro, sarà hashata con Sha256)

                    RedirectUris = { "https://localhost:7040/swagger/oauth2-redirect.html" }, // URI di redirect usato da Swagger UI
                    AllowedCorsOrigins = { "https://localhost:7040", "https://localhost:5001" },
                    AllowedScopes = { "openid", "profile", "api1", "offline_access" },        // Scope che il client può richiedere
                    AllowOfflineAccess = true                                                 // Permette il refresh token
                }


                //new Client
                //{
                //    ClientId = "client_app",
                //    AllowedGrantTypes = GrantTypes.ClientCredentials,
                //    ClientSecrets = { new Secret("secret".Sha256()) },
                //    AllowedScopes = { "api1" }
                //    // Puoi aggiungere qui anche i RedirectUris, CORSOrigins, ecc.
                //}
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
