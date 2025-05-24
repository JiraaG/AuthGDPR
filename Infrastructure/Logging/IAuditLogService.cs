using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Infrastructure.Logging
{
    /// <summary>
    /// Interfaccia per il servizio di audit log.
    /// Definisce il contratto per la registrazione degli eventi critici nel sistema.
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Registra un evento di audit log.
        /// </summary>
        /// <param name="userId">ID reale dell'utente (verrà pseudonimizzato, può essere null per eventi di sistema)</param>
        /// <param name="messageCategory">Categoria del messaggio (Info, Errore, ecc.)</param>
        /// <param name="actionType">Tipo di azione eseguita</param>
        /// <param name="entityName">Nome dell'entità coinvolta</param>
        /// <param name="entityId">ID dell'entità coinvolta</param>
        /// <param name="description">Descrizione dettagliata dell'evento</param>
        /// <param name="ipAddress">Indirizzo IP del client (opzionale)</param>
        /// <param name="traceId">Identificativo della traccia della richiesta (opzionale)</param>
        Task LogEventAsync(
                Guid? userId,
                MessageCategory messageCategory,
                ActionType actionType,
                string entityName,
                string entityId,
                string description = null,
                string ipAddress = null,
                string traceId = null);
    }
}
