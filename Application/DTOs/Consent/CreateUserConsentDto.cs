using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Application.DTOs.Consent
{
    public class CreateUserConsentDto
    {
        // Il tipo di consenso espresso dall'utente (es. Accettazione per marketing, analytics, etc.)
        public ConsentType ConsentType { get; set; }

        // Riferimento alla versione specifica della policy che l'utente ha visualizzato ed approvato
        public Guid ConsentPolicyId { get; set; }

        // Questi dati possono essere rilevati automaticamente in fase di richiesta (ad es. dal contesto HTTP)
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
