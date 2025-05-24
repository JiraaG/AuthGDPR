using AuthGDPR.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuthGDPR.Application.DTOs.DataSubjectRequest
{
    public class UpdateDataSubjectRequestStatusDto
    {
        [Required]
        public RequestStatus Status { get; set; }

        [Required]
        public ResponseIdentity ResponseIdentity { get; set; }

        public string? Description { get; set; }
    }

}
