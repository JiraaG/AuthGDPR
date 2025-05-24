using System.Net;
using AuthGDPR.Application.DTOs.DataSubjectRequest;
using AuthGDPR.Application.Interfaces;
using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Domain.Enums;
using AuthGDPR.Infrastructure.Persistance;
using Duende.IdentityModel;
using Microsoft.EntityFrameworkCore;

namespace AuthGDPR.Application.Services
{
    /// <summary>
    /// Implementazione del servizio per la gestione delle richieste dei soggetti interessati (Data Subject Requests).
    /// Gestisce la creazione, ricerca, aggiornamento e cancellazione delle richieste GDPR.
    /// </summary>
    public class DataSubjectRequestService : IDataSubjectRequestService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Costruttore del servizio.
        /// </summary>
        public DataSubjectRequestService(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _appDbContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Crea una nuova richiesta e la salva nel database.
        /// </summary>
        public async Task<DataSubjectRequestDto> CreateAsync(CreateDataSubjectRequestDto dto)
        {
            // Crea una nuova entità DataSubjectRequest
            var newRequest = new DataSubjectRequest
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                RequestType = dto.RequestType,
                RequestDate = DateTime.UtcNow,
                ProcessedAt = null,
                Status = RequestStatus.Pending,
                ResponseIdentity = dto.ResponseIdentity,
                Description = dto.Description,
                TraceIdentifier = _httpContextAccessor.HttpContext?.TraceIdentifier
                                    ?? Guid.NewGuid().ToString(),
                IPAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
            };

            // Aggiunge la richiesta al contesto EF
            _appDbContext.DataSubjectRequests.Add(newRequest);
            await _appDbContext.SaveChangesAsync();

            // Restituisce il DTO della richiesta creata
            return new DataSubjectRequestDto
            {
                Id = newRequest.Id,
                UserId = newRequest.UserId,
                RequestType = newRequest.RequestType,
                RequestDate = newRequest.RequestDate,
                ProcessedAt = newRequest.ProcessedAt,
                Status = newRequest.Status,
                ResponseIdentity = newRequest.ResponseIdentity,
                Description = newRequest.Description,
                TraceIdentifier = newRequest.TraceIdentifier,
                IPAddress = newRequest.IPAddress
            };
        }

        /// <summary>
        /// Recupera una richiesta tramite ID.
        /// </summary>
        public async Task<DataSubjectRequestDto?> GetByIdAsync(Guid id)
        {
            // Cerca la richiesta nel database
            var entity = await _appDbContext.DataSubjectRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null)
                return null;

            // Restituisce il DTO della richiesta trovata
            return new DataSubjectRequestDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                RequestType = entity.RequestType,
                RequestDate = entity.RequestDate,
                ProcessedAt = entity.ProcessedAt,
                Status = entity.Status,
                ResponseIdentity = entity.ResponseIdentity,
                Description = entity.Description,
                TraceIdentifier = entity.TraceIdentifier,
                IPAddress = entity.IPAddress
            };
        }

        /// <summary>
        /// Recupera tutte le richieste associate a un utente.
        /// </summary>
        public async Task<IEnumerable<DataSubjectRequestDto>> GetByUserIdAsync(Guid userId)
        {
            // Filtra le richieste per UserId
            return await _appDbContext.DataSubjectRequests
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Select(entity => new DataSubjectRequestDto
                {
                    Id = entity.Id,
                    UserId = entity.UserId,
                    RequestType = entity.RequestType,
                    RequestDate = entity.RequestDate,
                    ProcessedAt = entity.ProcessedAt,
                    Status = entity.Status,
                    ResponseIdentity = entity.ResponseIdentity,
                    Description = entity.Description,
                    TraceIdentifier = entity.TraceIdentifier,
                    IPAddress = entity.IPAddress
                })
                .ToListAsync();
        }

        /// <summary>
        /// Recupera tutte le richieste filtrate per stato.
        /// </summary>
        public async Task<IEnumerable<DataSubjectRequestDto>> GetByStatusAsync(RequestStatus status)
        {
            // Filtra le richieste per Status
            return await _appDbContext.DataSubjectRequests
                .AsNoTracking()
                .Where(r => r.Status == status)
                .Select(entity => new DataSubjectRequestDto
                {
                    Id = entity.Id,
                    UserId = entity.UserId,
                    RequestType = entity.RequestType,
                    RequestDate = entity.RequestDate,
                    ProcessedAt = entity.ProcessedAt,
                    Status = entity.Status,
                    ResponseIdentity = entity.ResponseIdentity,
                    Description = entity.Description,
                    TraceIdentifier = entity.TraceIdentifier,
                    IPAddress = entity.IPAddress
                })
                .ToListAsync();
        }

        /// <summary>
        /// Aggiorna lo stato di una richiesta esistente.
        /// </summary>
        public async Task<DataSubjectRequestDto?> UpdateStatusAsync(Guid id, UpdateDataSubjectRequestStatusDto dto)
        {
            // Cerca la richiesta da aggiornare
            var entity = await _appDbContext.DataSubjectRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (entity == null)
                return null;

            // Aggiorna i campi della richiesta
            entity.Status = dto.Status;
            entity.ProcessedAt = DateTime.UtcNow;
            entity.ResponseIdentity = dto.ResponseIdentity;
            entity.Description = dto.Description;

            await _appDbContext.SaveChangesAsync();

            // Restituisce il DTO aggiornato
            return new DataSubjectRequestDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                RequestType = entity.RequestType,
                RequestDate = entity.RequestDate,
                ProcessedAt = entity.ProcessedAt,
                Status = entity.Status,
                ResponseIdentity = entity.ResponseIdentity,
                Description = entity.Description,
                TraceIdentifier = entity.TraceIdentifier,
                IPAddress = entity.IPAddress
            };
        }

        /// <summary>
        /// Elimina una richiesta dal database.
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            // Cerca la richiesta da eliminare
            var entity = await _appDbContext.DataSubjectRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (entity == null)
                return false;

            // Rimuove la richiesta dal contesto EF
            _appDbContext.DataSubjectRequests.Remove(entity);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
