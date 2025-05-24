using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Domain.Entities.Consent
{
    public class UserConsentHistory
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Riferimento al record di UserConsent cui si riferisce la modifica.
        /// </summary>
        [Required]
        public Guid UserConsentId { get; set; }

        /// <summary>
        /// Data e ora della modifica.
        /// </summary>
        [Required]
        public DateTime ChangeDate { get; set; }

        [Required]
        public ConsentType ConsentType { get; set; }

        /// <summary>
        /// Tipo di operazione eseguita (es. Created, Modified, Revoked).
        /// </summary>
        [Required]
        public ConsentChangeType ChangeType { get; set; }

        /// <summary>
        /// User Agent o informazioni sul browser/dispositivo al momento della modifica.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Indirizzo IP registrato durante la modifica.
        /// </summary>
        public string? IPAddress { get; set; }

        // Proprietà di navigazione
        [ForeignKey("UserConsentId")]
        public UserConsent UserConsent { get; set; } = default!;
    }
}
