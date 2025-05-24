using System;
using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Domain.Enums;
using AuthGDPR.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AuthGDPR.Infrastructure.Logging
{
    /// <summary>
    /// Servizio per la gestione e la persistenza dei log di audit.
    /// Registra le operazioni critiche nel database e su Serilog per garantire tracciabilità e conformità.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _appDbContext;
        private readonly PseudonymizerService _pseudonymizerService;

        /// <summary>
        /// Costruttore del servizio di audit log.
        /// </summary>
        public AuditLogService(AppDbContext appDbContext, PseudonymizerService pseudonymizerService)
        {
            _appDbContext = appDbContext;
            _pseudonymizerService = pseudonymizerService;
        }

        /// <summary>
        /// Registra un evento di audit log nel database e su Serilog.
        /// L'ID utente viene pseudonimizzato per la privacy.
        /// </summary>
        /// <param name="userId">ID reale dell'utente (verrà pseudonimizzato, può essere null per eventi di sistema)</param>
        /// <param name="messageCategory">Categoria del messaggio (Info, Errore, ecc.)</param>
        /// <param name="actionType">Tipo di azione eseguita</param>
        /// <param name="entityName">Nome dell'entità coinvolta</param>
        /// <param name="entityId">ID dell'entità coinvolta</param>
        /// <param name="description">Descrizione dettagliata dell'evento</param>
        /// <param name="ipAddress">Indirizzo IP del client (opzionale)</param>
        /// <param name="traceId">Identificativo della traccia della richiesta (opzionale)</param>
        public async Task LogEventAsync(
                            Guid? userId,
                            MessageCategory messageCategory,
                            ActionType actionType,
                            string entityName,
                            string entityId,
                            string description = null,
                            string ipAddress = null,
                            string traceId = null)
        {
            // Se l'ID utente è presente, applica la pseudonimizzazione
            Guid? pseudonymizedUserId = null;
            if (userId.HasValue)
            {
                // Pseudonimizza l'ID utente per la privacy
                pseudonymizedUserId = _pseudonymizerService.GetPseudonymizedUserId(userId.Value);
            }

            // Crea il record di audit log
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),             // Genera l'ID del log
                MessageCategory = messageCategory,
                Timestamp = DateTime.UtcNow,
                UserId = pseudonymizedUserId,      // L'ID utente pseudonimizzato (può essere null per eventi di sistema)
                ActionType = actionType,           // L'azione eseguita
                EntityName = entityName,           // Il nome dell'entità interessata
                EntityId = entityId,               // L'ID dell'entità modificata o coinvolta
                Description = description,         // Descrizione dell'evento
                IPAddress = ipAddress,             // L'indirizzo IP dell'utente (se disponibile)
                TraceId = traceId
            };

            // Registra il log come Information su Serilog
            Log.Information("{@AuditLog}", auditLog);

            // Salva il log nel database
            _appDbContext.AuditLogs.Add(auditLog);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
