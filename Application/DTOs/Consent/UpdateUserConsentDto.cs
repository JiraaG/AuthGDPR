using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Application.DTOs.Consent
{
    public class UpdateUserConsentDto
    {
        // Se l'utente modifica la propria scelta (per esempio da "accettato" a "rifiutato")
        public ConsentType ConsentType { get; set; }

        // Se necessario, si può indicare una nuova policy di riferimento (oppure la stessa)
        public Guid ConsentPolicyId { get; set; }

        // Informazioni aggiornate (opzionali) relative al contesto della modifica
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
