using AuthGDPR.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuthGDPR.Domain.Entities.Auth
{
    /// <summary>
    /// Entità che rappresenta un record di audit log.
    /// Memorizza le operazioni critiche effettuate nel sistema per ricostruire la cronologia delle attività,
    /// controllare operazioni non autorizzate e garantire la conformità normativa.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Identificativo univoco del log.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Categoria del messaggio (es. Info, Errore, Successo).
        /// </summary>
        [Required]
        public MessageCategory MessageCategory { get; set; }

        /// <summary>
        /// Data e ora dell'evento loggato (UTC).
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Identificativo (pseudonimizzato) dell'utente che ha effettuato l'azione.
        /// Può essere null per eventi di sistema.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Tipo di azione eseguita (es. Created, Updated, Login, ecc.).
        /// </summary>
        [Required]
        public ActionType ActionType { get; set; }

        /// <summary>
        /// Nome dell'entità coinvolta nell'operazione (es. "ApplicationUser", "DataSubjectRequest").
        /// </summary>
        [Required]
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Identificativo dell'entità coinvolta nell'operazione (es. ID utente, ID richiesta).
        /// </summary>
        [Required]
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione dettagliata dell'evento o dell'operazione.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Indirizzo IP del client che ha effettuato l'operazione (se disponibile).
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// Identificativo della traccia della richiesta (traceId) per correlare i log.
        /// </summary>
        [Required]
        public string TraceId { get; set; } = string.Empty;
    }
}
