using AuthGDPR.Application.Interfaces;
using AuthGDPR.Domain.Entities.Consent;
using AuthGDPR.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace AuthGDPR.Application.Services
{
    public class ConsentPolicyService : IConsentPolicyService
    {
        private readonly AppDbContext _context;

        public ConsentPolicyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConsentPolicy>> GetAllAsync()
        {
            return await _context.ConsentPolicies
                .Where(cp => !_context.ConsentPolicies.Any(p => p.PreviousConsentPolicyId == cp.Id))
                .ToListAsync();
        }

        public async Task<ConsentPolicy?> GetByIdAsync(Guid id)
        {
            return await _context.ConsentPolicies.FindAsync(id);
        }

        public async Task<ConsentPolicy> CreateAsync(ConsentPolicy consentPolicy)
        {
            _context.ConsentPolicies.Add(consentPolicy);
            await _context.SaveChangesAsync();
            return consentPolicy;
        }

        /// <summary>
        /// Invece di aggiornare il record esistente, crea una nuova versione che fa riferimento al record precedente.
        /// </summary>
        public async Task<ConsentPolicy?> UpdateAsync(Guid id, ConsentPolicy updatedPolicy)
        {
            var existingPolicy = await _context.ConsentPolicies.FindAsync(id);
            if (existingPolicy == null)
            {
                return null;
            }

            // Creiamo una nuova riga per la nuova versione
            updatedPolicy.Id = Guid.NewGuid();
            updatedPolicy.PreviousConsentPolicyId = existingPolicy.Id;
            _context.ConsentPolicies.Add(updatedPolicy);

            await _context.SaveChangesAsync();
            return updatedPolicy;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var policy = await _context.ConsentPolicies.FindAsync(id);
            if (policy == null)
            {
                return false;
            }

            _context.ConsentPolicies.Remove(policy);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Restituisce la policy "attiva": quella per cui EffectiveDate è valida (<= ora attuale)
        /// e per la quale non esiste una versione successiva (cioè non appare come PreviousConsentPolicyId di un altro record).
        /// Se ci fossero più catene, ne viene restituita quella con EffectiveDate più recente.
        /// </summary>
        public async Task<ConsentPolicy?> GetActivePolicyAsync()
        {
            var now = DateTime.UtcNow;

            var activePolicy = await _context.ConsentPolicies
                .Where(cp => cp.EffectiveDate <= now &&
                            // Nessun record che riferisca questo cp come versione precedente
                            !_context.ConsentPolicies.Any(p => p.PreviousConsentPolicyId == cp.Id))
                .OrderByDescending(cp => cp.EffectiveDate) // Prendiamo quella con l'effettività più recente
                .FirstOrDefaultAsync();

            return activePolicy;
        }

        public async Task<IEnumerable<ConsentPolicy>> GetActiveMandatoryPoliciesAsync()
        {
            var now = DateTime.UtcNow;
            // Recupera tutte le policy attive obbligatorie che non sono superate da versioni successive
            var policies = await _context.ConsentPolicies
                              .Where(cp => cp.EffectiveDate <= now &&
                                           cp.IsMandatory &&
                                           !_context.ConsentPolicies.Any(p => p.PreviousConsentPolicyId == cp.Id))
                              .ToListAsync();
            return policies;
        }
    }
}
