using AuthGDPR.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuthGDPR.Application.DTOs.DataSubjectRequest
{
    public class CreateDataSubjectRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public RequestType RequestType { get; set; }

        [Required]
        public ResponseIdentity ResponseIdentity { get; set; }

        // La descrizione è facoltativa e serve per dare ulteriori dettagli all'operazione
        public string? Description { get; set; }
    }

}
