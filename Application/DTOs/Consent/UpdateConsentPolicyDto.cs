using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Application.DTOs.Consent
{
    public class UpdateConsentPolicyDto
    {
        public string Version { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EffectiveDate { get; set; }
        public ConsentType ConsentType { get; set; }
        public bool IsMandatory { get; set; } = false;
    }
}
