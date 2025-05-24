using AuthGDPR.Domain.Entities.Consent;

namespace AuthGDPR.Application.Interfaces
{
    public interface IUserConsentService
    {
        // Restituisce tutti i consensi per un determinato utente
        Task<IEnumerable<UserConsent>> GetAllByUserIdAsync(Guid userId);

        // Recupera un singolo consenso specifico per utente
        Task<UserConsent?> GetByIdAsync(Guid id, Guid userId);

        // Crea un nuovo consenso per l'utente
        Task<UserConsent> CreateAsync(UserConsent userConsent);

        // Aggiorna un consenso esistente (o, se preferisci lo storico, crea una nuova riga che faccia riferimento al precedente)
        Task<UserConsent?> UpdateAsync(Guid id, UserConsent updatedConsent, Guid userId);

        // Se necessario, elimina (o segna come revocato) un consenso
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
