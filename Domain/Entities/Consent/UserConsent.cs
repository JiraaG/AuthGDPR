using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthGDPR.Domain.Entities.Consent
{
    // 2. Consenso dell'utente
    public class UserConsent
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public ConsentType ConsentType { get; set; }

        /// <summary>
        /// Riferimento alla versione specifica del consenso approvato, gestita nella tabella ConsentPolicy.
        /// </summary>
        [Required]
        public Guid ConsentPolicyId { get; set; }

        [ForeignKey("ConsentPolicyId")]
        public ConsentPolicy ConsentPolicy { get; set; } = default!;

        /// <summary>
        /// Data in cui l’utente ha espresso il consenso.
        /// </summary>
        [Required]
        public DateTime ConsentDate { get; set; }

        /// <summary>
        /// Indirizzo IP dell'utente al momento del consenso.
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// Data di creazione del record.
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Data dell'ultima modifica, se presente.
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// User Agent (browser/dispositivo) al momento del consenso.
        /// </summary>
        public string? UserAgent { get; set; }

        // Riferimento all’utente dell’applicazione.
        public ApplicationUser User { get; set; } = default!;

        // Campo per collegare la riga precedente, se presente
        public Guid? PreviousUserConsentId { get; set; }

        [ForeignKey("PreviousUserConsentId")]
        public UserConsent? PreviousUserConsent { get; set; }
    }
}
