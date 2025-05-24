using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Domain.Entities.Consent
{
    public class ConsentPolicy
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Versione univoca del consenso (es. "v1.0", "v2.1", ecc.).
        /// </summary>
        [Required]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Testo completo dell’informativa da mostrare all’utente.
        /// </summary>
        [Required]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione sintetica o riassunto delle finalità del consenso.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Data da cui questa versione del consenso è effettiva.
        /// </summary>
        [Required]
        public DateTime EffectiveDate { get; set; }

        // Nuovo campo per indicare se la policy è obbligatoria
        [Required]
        public bool IsMandatory { get; set; } = false;

        [Required]
        public ConsentType ConsentType { get; set; }

        /// <summary>
        /// Riferimento alla versione precedente (se presente).
        /// Se questo campo è valorizzato, il record corrente rappresenta una nuova versione derivata dal precedente.
        /// </summary>
        public Guid? PreviousConsentPolicyId { get; set; }

        [ForeignKey("PreviousConsentPolicyId")]
        public ConsentPolicy? PreviousConsentPolicy { get; set; }
    }
}
