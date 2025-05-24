using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Domain.Enums;
using AuthGDPR.Infrastructure;
using AuthGDPR.Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AuthGDPR.Api.Controller.Base
{
    /// <summary>
    /// Controller base per tutte le API.
    /// Fornisce funzionalità comuni come la gestione centralizzata degli errori e il logging delle operazioni critiche.
    /// </summary>
    public abstract class ApiBaseController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        /// <summary>
        /// Costruttore del controller base.
        /// </summary>
        protected ApiBaseController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Metodo helper che, ricevendo i dati di errore, logga l'evento e crea un oggetto ProblemDetails da restituire.
        /// </summary>
        /// <param name="userId">ID dell'utente (eventualmente null)</param>
        /// <param name="messageCategory">Categoria del messaggio</param>
        /// <param name="actionType">Tipo di azione</param>
        /// <param name="entityName">Nome dell'entità interessata</param>
        /// <param name="entityId">ID dell'entità interessata</param>
        /// <param name="errorEnum">Enum che rappresenta il tipo di errore</param>
        /// <param name="ipAddress">Indirizzo IP del client (facoltativo)</param>
        /// <param name="errorModelState">Dettagli errori ModelState (facoltativo)</param>
        /// <param name="statusCode">Codice HTTP da restituire (default 400)</param>
        /// <returns>Risposta contenente il ProblemDetails</returns>
        protected async Task<IActionResult> CreateErrorResponseAsync(
                                                            int statusCode,
                                                            MessageCategory messageCategory,
                                                            ActionType actionType,
                                                            Guid userId,
                                                            string entityName,
                                                            string entityId,
                                                            ApiMessages errorEnum,
                                                            string ipAddress = null,
                                                            string errorModelState = null)
        {
            // Se non viene passato l'indirizzo IP, lo acquisisce dal contesto della richiesta
            string ip = ipAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            // Se viene passato l'errore del modelState, lo utilizza, altrimenti usa l'errore enum
            var errorMessage = errorModelState == null ? errorEnum.GetDescription() : errorModelState;

            // Registra l'evento tramite il servizio di audit
            await _auditLogService.LogEventAsync(
                userId,
                messageCategory,
                actionType,
                entityName,
                entityId,
                errorMessage,
                ip,
                HttpContext.TraceIdentifier
            );

            var errors = new Dictionary<string, string[]>
                {
                    { $"{errorEnum.ToString()}", new [] { $"{errorMessage}" } }
                };

            // Costruisce l'oggetto ProblemDetails con i dati gestiti
            var problemDetails = new ValidationProblemDetails(errors)
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-status-codes",
                Title = errorEnum.ToString(),
                Status = statusCode,
                Instance = HttpContext.Request.Path
            };
            // Aggiungi il traceId nella sezione Extensions
            problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;

            // Restituisce la risposta con il ProblemDetails
            return StatusCode(statusCode, problemDetails);
        }

        /// <summary>
        /// Metodo helper che, ricevendo i dati di successo, logga l'evento e restituisce l'oggetto passato con lo status code specificato.
        /// </summary>
        /// <typeparam name="T">Tipo dell'oggetto da restituire</typeparam>
        /// <param name="statusCode">Codice HTTP da restituire (es. 200, 201, ecc.)</param>
        /// <param name="messageCategory">Categoria del messaggio (es. Successo, Info)</param>
        /// <param name="actionType">Tipo di azione</param>
        /// <param name="userId">ID dell'utente (eventualmente null)</param>
        /// <param name="entityName">Nome dell'entità interessata</param>
        /// <param name="entityId">ID dell'entità interessata</param>
        /// <param name="description">Descrizione dell'operazione</param>
        /// <param name="ipAddress">Indirizzo IP del client (facoltativo)</param>
        /// <param name="result">Oggetto da restituire</param>
        /// <returns>Risposta contenente l'oggetto e lo status code</returns>
        protected async Task<IActionResult> CreateSuccessResponseAsync<T>(
            int statusCode,
            MessageCategory messageCategory,
            ActionType actionType,
            Guid userId,
            string entityName,
            string entityId,
            string description,
            T result,
            string ipAddress = null)
        {
            // Se non viene passato l'indirizzo IP, lo acquisisce dal contesto della richiesta
            string ip = ipAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            // Logga l'evento di successo tramite il servizio di audit
            await _auditLogService.LogEventAsync(
                userId,
                messageCategory,
                actionType,
                entityName,
                entityId,
                description,
                ip,
                HttpContext.TraceIdentifier
            );

            // Restituisce la risposta con lo status code e l'oggetto passato
            return StatusCode(statusCode, result);
        }


    }
}
