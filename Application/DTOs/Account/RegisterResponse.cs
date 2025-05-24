using System.ComponentModel.DataAnnotations;

namespace AuthGDPR.Application.DTOs.Account
{
    public class RegisterResponse
    {
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid PseudonymizedUserId { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
