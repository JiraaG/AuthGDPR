using AuthGDPR.Application.DTOs.Consent;
using System.ComponentModel.DataAnnotations;

namespace AuthGDPR.Application.DTOs.Account
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        // Campo opzionale
        public string? Address { get; set; }

        // Nuovi campi per i consensi
        public IEnumerable<UserConsentInputDto> Consents { get; set; } = new List<UserConsentInputDto>();
    }
}
