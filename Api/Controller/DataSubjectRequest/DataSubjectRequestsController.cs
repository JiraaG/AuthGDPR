using AuthGDPR.Api.Controller.Base;
using AuthGDPR.Application.DTOs.DataSubjectRequest;
using AuthGDPR.Application.Interfaces;
using AuthGDPR.Domain.Enums;
using AuthGDPR.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthGDPR.Api.Controller.DataSubjectRequest
{
    /// <summary>
    /// Controller per la gestione delle richieste dei soggetti interessati (Data Subject Requests) secondo il GDPR.
    /// Permette la creazione, consultazione e aggiornamento delle richieste relative ai dati personali degli utenti.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/account")]
    public class DataSubjectRequestsController : ApiBaseController
    {
        private readonly IDataSubjectRequestService _dataSubjectRequestSuservice;

        /// <summary>
        /// Costruttore del controller.
        /// </summary>
        public DataSubjectRequestsController(IDataSubjectRequestService dataSubjectRequestSuservice, IAuditLogService auditLogService)
            : base(auditLogService)
        {
            _dataSubjectRequestSuservice = dataSubjectRequestSuservice;
        }

        /// <summary>
        /// Registra una nuova richiesta del soggetto interessato.
        /// </summary>
        /// <param name="dto">DTO con i dati della richiesta</param>
        /// <returns>La richiesta creata</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDataSubjectRequestDto dto)
        {
            // Validazione del modello in ingresso
            if (!ModelState.IsValid)
            {
                var errorList = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                string errorDescription = string.Join(" | ", errorList.SelectMany(x => x.Errors));

                // Restituisce errore dettagliato e loggato
                return await CreateErrorResponseAsync(
                    StatusCodes.Status400BadRequest,
                    MessageCategory.Errore,
                    ActionType.Created,
                    Guid.Empty,
                    "DataSubjectRequests",
                    "0",
                    ApiMessages.ErroreInputDati,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    errorDescription
                );
            }

            // Crea la richiesta tramite il servizio
            var createdRequest = await _dataSubjectRequestSuservice.CreateAsync(dto);

            // Restituisco l'oggetto creato loggando l'evento
            return await CreateSuccessResponseAsync(
                StatusCodes.Status201Created,
                MessageCategory.Successo,
                ActionType.Created,
                createdRequest.UserId,
                "DataSubjectRequests",
                createdRequest.Id.ToString(),
                "Richiesta creata con successo",
                createdRequest,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            //return Ok(createdRequest);
        }

        /// <summary>
        /// Recupera i dettagli di una richiesta specifica tramite ID.
        /// </summary>
        /// <param name="id">ID della richiesta</param>
        /// <returns>La richiesta trovata o errore 404</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Recupera la richiesta dal servizio
            var request = await _dataSubjectRequestSuservice.GetByIdAsync(id);

            if (request == null)
                return NotFound(new { message = "Richiesta non trovata" });

            return Ok(request);
        }

        /// <summary>
        /// Recupera tutte le richieste associate a un determinato utente.
        /// </summary>
        /// <param name="userId">ID dell'utente</param>
        /// <returns>Lista delle richieste</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            // Recupera le richieste per utente
            IEnumerable<DataSubjectRequestDto> requests = await _dataSubjectRequestSuservice.GetByUserIdAsync(userId);

            return Ok(requests);
        }

        /// <summary>
        /// Recupera le richieste filtrate per stato.
        /// </summary>
        /// <param name="status">Stato della richiesta</param>
        /// <returns>Lista delle richieste filtrate</returns>
        [HttpGet]
        public async Task<IActionResult> GetByStatus([FromQuery] RequestStatus status)
        {
            // Recupera le richieste per stato
            IEnumerable<DataSubjectRequestDto> requests = await _dataSubjectRequestSuservice.GetByStatusAsync(status);
            return Ok(requests);
        }

        /// <summary>
        /// Aggiorna lo stato di una richiesta esistente.
        /// </summary>
        /// <param name="id">ID della richiesta</param>
        /// <param name="dto">DTO con i nuovi dati di stato</param>
        /// <returns>La richiesta aggiornata o errore</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDataSubjectRequestStatusDto dto)
        {
            // Validazione del modello in ingresso
            if (!ModelState.IsValid)
            {
                var errorList = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                string errorDescription = string.Join(" | ", errorList.SelectMany(x => x.Errors));

                // Restituisce errore dettagliato e loggato
                return await CreateErrorResponseAsync(
                    StatusCodes.Status400BadRequest,
                    MessageCategory.Errore,
                    ActionType.Updated,
                    Guid.Empty,
                    "DataSubjectRequests",
                    "0",
                    ApiMessages.ErroreInputDati,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    errorDescription
                );
            }

            // Aggiorna la richiesta tramite il servizio
            var updated = await _dataSubjectRequestSuservice.UpdateStatusAsync(id, dto);
            if (updated == null)
            {
                // Restituisce errore se la richiesta non esiste
                return await CreateErrorResponseAsync(
                    StatusCodes.Status400BadRequest,
                    MessageCategory.Errore,
                    ActionType.Updated,
                    Guid.Empty,
                    "DataSubjectRequests",
                    "0",
                    ApiMessages.ErroreInputDati,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
            }

            return Ok(updated);
        }

        // Metodo opzionale per eliminare una richiesta (commentato per policy applicativa).
        // DELETE: api/datasubjectrequests/{id}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    var result = await _dataSubjectRequestSuservice.DeleteAsync(id);
        //    if (!result)
        //        return NotFound(new { message = "Richiesta non trovata" });
        //    return NoContent();
        //}
    }
}
