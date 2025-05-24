using AuthGDPR.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuthGDPR.Domain.Entities.Auth
{
    /// <summary>
    /// Entità che rappresenta una richiesta di un soggetto interessato (Data Subject Request) secondo il GDPR.
    /// Utilizzata per tracciare richieste di accesso, rettifica, cancellazione, portabilità, ecc. dei dati personali.
    /// </summary>
    public class DataSubjectRequest
    {
        /// <summary>
        /// Identificativo univoco della richiesta.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificativo dell'utente che ha effettuato la richiesta.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tipo di richiesta (accesso, rettifica, cancellazione, portabilità, ecc.).
        /// </summary>
        [Required]
        public RequestType RequestType { get; set; }

        /// <summary>
        /// Data e ora in cui la richiesta è stata effettuata.
        /// </summary>
        [Required]
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Data e ora in cui la richiesta è stata processata (se presente).
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Stato attuale della richiesta (Pending, InProgress, Completed, Rejected).
        /// </summary>
        [Required]
        public RequestStatus Status { get; set; }

        /// <summary>
        /// Identità che ha risposto alla richiesta (Utente, Amministratore).
        /// </summary>
        [Required]
        public ResponseIdentity ResponseIdentity { get; set; }

        /// <summary>
        /// Descrizione aggiuntiva fornita dall'utente o dall'amministratore.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Identificatore della traccia della richiesta (per audit/logging).
        /// </summary>
        [Required]
        public string TraceIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Indirizzo IP da cui è stata effettuata la richiesta.
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// Riferimento all'utente che ha effettuato la richiesta.
        /// </summary>
        public ApplicationUser User { get; set; } = default!;
    }
}
