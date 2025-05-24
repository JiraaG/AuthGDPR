using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Application.DTOs.Consent
{
    public class UserConsentInputDto
    {
        public Guid ConsentPolicyId { get; set; }
        public bool Accepted { get; set; }
        // Optional: potresti includere altri dati come ConsentType se la logica aziendale lo richiede
        public ConsentType ConsentType { get; set; }
    }
}
