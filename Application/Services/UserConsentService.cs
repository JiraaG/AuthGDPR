using AuthGDPR.Application.Interfaces;
using AuthGDPR.Domain.Entities.Consent;
using AuthGDPR.Domain.Enums;
using AuthGDPR.Infrastructure.Logging;
using AuthGDPR.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace AuthGDPR.Application.Services
{
    public class UserConsentService : IUserConsentService
    {
        private readonly AppDbContext _context;
        private readonly PseudonymizerService _pseudonymizerService;

        public UserConsentService(AppDbContext context, PseudonymizerService pseudonymizerService)
        {
            _context = context;
            _pseudonymizerService = pseudonymizerService;
        }

        public async Task<IEnumerable<UserConsent>> GetAllByUserIdAsync(Guid userId)
        {
            var user = await _pseudonymizerService.GetUserByPseudonymizedIdAsync(userId);

            return await _context.UserConsents
                .Include(uc => uc.ConsentPolicy)
                .Where(uc => uc.UserId == user.Id)
                .ToListAsync();
        }

        public async Task<UserConsent?> GetByIdAsync(Guid id, Guid userId)
        {
            var user = await _pseudonymizerService.GetUserByPseudonymizedIdAsync(userId);

            return await _context.UserConsents
                .Include(uc => uc.ConsentPolicy)
                .FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == user.Id);
        }

        // Metodo per la creazione del consenso
        public async Task<UserConsent> CreateAsync(UserConsent userConsent)
        {
            userConsent.Id = Guid.NewGuid();
            userConsent.CreatedDate = DateTime.UtcNow;
            userConsent.ConsentDate = DateTime.UtcNow;

            _context.UserConsents.Add(userConsent);
            await _context.SaveChangesAsync();

            // Registra la creazione nello storico con ChangeType "Created"
            var history = new UserConsentHistory
            {
                Id = Guid.NewGuid(),
                UserConsentId = userConsent.Id,
                ChangeDate = DateTime.UtcNow,
                ConsentType = userConsent.ConsentType,
                ChangeType = ConsentChangeType.Created,
                IPAddress = userConsent.IPAddress,
                UserAgent = userConsent.UserAgent
            };

            _context.UserConsentHistories.Add(history);
            await _context.SaveChangesAsync();

            return userConsent;
        }

        // Metodo per aggiornare (in modo da creare un nuovo record e mantenere lo storico)
        public async Task<UserConsent?> UpdateAsync(Guid id, UserConsent updatedConsent, Guid userId)
        {
            var user = await _pseudonymizerService.GetUserByPseudonymizedIdAsync(userId);

            // Recupera il consenso esistente per l'utente
            var existingConsent = await _context.UserConsents
                .FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == user.Id);
            if (existingConsent == null)
                return null;

            // Collega la nuova riga alla precedente
            updatedConsent.PreviousUserConsentId = existingConsent.Id;

            // Crea la nuova versione del consenso
            updatedConsent.Id = Guid.NewGuid();
            updatedConsent.UserId = user.Id;
            updatedConsent.CreatedDate = DateTime.UtcNow;
            updatedConsent.ConsentDate = DateTime.UtcNow;
            _context.UserConsents.Add(updatedConsent);
            await _context.SaveChangesAsync();

            // Registra la modifica nello storico con ChangeType "Modified"
            var history = new UserConsentHistory
            {
                Id = Guid.NewGuid(),
                UserConsentId = updatedConsent.Id, // Collega lo storico al nuovo record
                ChangeDate = DateTime.UtcNow,
                ConsentType = updatedConsent.ConsentType,
                ChangeType = ConsentChangeType.Modified,
                IPAddress = updatedConsent.IPAddress,
                UserAgent = updatedConsent.UserAgent
            };

            _context.UserConsentHistories.Add(history);
            await _context.SaveChangesAsync();

            return updatedConsent;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var user = await _pseudonymizerService.GetUserByPseudonymizedIdAsync(userId);

            var consent = await _context.UserConsents.FirstOrDefaultAsync(uc => uc.Id == id && uc.UserId == user.Id);
            if (consent == null)
                return false;

            _context.UserConsents.Remove(consent);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
