using System.ComponentModel.DataAnnotations;
using AuthGDPR.Domain.Entities.Consent;
using Microsoft.AspNetCore.Identity;

namespace AuthGDPR.Domain.Entities.Auth
{
    // 1. Utente (Data Subject)
    // Ora ApplicationUser eredita da IdentityUser<Guid>
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }

        [Required]
        public Guid PseudonymizedUserId { get; set; }

        // Relazioni GDPR
        public ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();
        public ICollection<DataSubjectRequest> DataSubjectRequests { get; set; } = new List<DataSubjectRequest>();
    }
}
