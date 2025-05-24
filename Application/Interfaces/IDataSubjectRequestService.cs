using AuthGDPR.Application.DTOs.DataSubjectRequest;
using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Application.Interfaces
{
    /// <summary>
    /// Interfaccia per la gestione delle richieste dei soggetti interessati (Data Subject Requests).
    /// Definisce le operazioni CRUD e di ricerca sulle richieste GDPR.
    /// </summary>
    public interface IDataSubjectRequestService
    {
        /// <summary>
        /// Crea una nuova richiesta del soggetto interessato.
        /// </summary>
        Task<DataSubjectRequestDto> CreateAsync(CreateDataSubjectRequestDto dto);

        /// <summary>
        /// Recupera una richiesta tramite ID.
        /// </summary>
        Task<DataSubjectRequestDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Recupera tutte le richieste associate a un utente.
        /// </summary>
        Task<IEnumerable<DataSubjectRequestDto>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Recupera tutte le richieste filtrate per stato.
        /// </summary>
        Task<IEnumerable<DataSubjectRequestDto>> GetByStatusAsync(RequestStatus status);

        /// <summary>
        /// Aggiorna lo stato di una richiesta esistente.
        /// </summary>
        Task<DataSubjectRequestDto?> UpdateStatusAsync(Guid id, UpdateDataSubjectRequestStatusDto dto);

        /// <summary>
        /// Elimina una richiesta (se previsto dalla policy).
        /// </summary>
        Task<bool> DeleteAsync(Guid id);
    }

}
