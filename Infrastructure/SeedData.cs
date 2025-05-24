using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;

namespace AuthGDPR.Infrastructure
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IApplicationBuilder app, ILogger logger, IHostEnvironment env)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            // Ottieni il ConfigurationDbContext
            var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

            // Applica le migrazioni (questo garantisce che lo schema sia aggiornato)
            configContext.Database.Migrate();
            logger.LogInformation("Database migrato per ConfigurationDbContext.");

            // -------------------- Seeding dei Clients --------------------
            if (!configContext.Clients.Any())
            {
                foreach (var client in ConfigSeedData.GetClients())
                {
                    // La conversione tramite ToEntity() popola anche i dati correlati (es. RedirectUris, GrantTypes, ecc.)
                    configContext.Clients.Add(client.ToEntity());
                }
                await configContext.SaveChangesAsync();
                logger.LogInformation("Seed dei Clients eseguito.");
            }
            else
            {
                logger.LogInformation("Clients già presenti, seed saltato.");
            }

            // ---------------- Seeding degli IdentityResources ----------------
            if (!configContext.IdentityResources.Any())
            {
                foreach (var resource in ConfigSeedData.GetIdentityResources())
                {
                    configContext.IdentityResources.Add(resource.ToEntity());
                }
                await configContext.SaveChangesAsync();
                logger.LogInformation("Seed degli IdentityResources eseguito.");
            }
            else
            {
                logger.LogInformation("IdentityResources già presenti, seed saltato.");
            }

            // -------------------- Seeding degli ApiScopes --------------------
            if (!configContext.ApiScopes.Any())
            {
                foreach (var scopeItem in ConfigSeedData.GetApiScopes())
                {
                    configContext.ApiScopes.Add(scopeItem.ToEntity());
                }
                await configContext.SaveChangesAsync();
                logger.LogInformation("Seed degli ApiScopes eseguito.");
            }
            else
            {
                logger.LogInformation("ApiScopes già presenti, seed saltato.");
            }

            // -------------------- Seeding degli ApiResources --------------------
            if (!configContext.ApiResources.Any())
            {
                foreach (var apiResource in ConfigSeedData.GetApiResources())
                {
                    configContext.ApiResources.Add(apiResource.ToEntity());
                }
                await configContext.SaveChangesAsync();
                logger.LogInformation("Seed degli ApiResources eseguito.");
            }
            else
            {
                logger.LogInformation("ApiResources già presenti, seed saltato.");
            }
        }

    }
}
