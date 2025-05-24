using AuthGDPR.Domain.Entities.Consent;

namespace AuthGDPR.Application.Interfaces
{
    public interface IConsentPolicyService
    {
        Task<IEnumerable<ConsentPolicy>> GetAllAsync();
        Task<ConsentPolicy?> GetByIdAsync(Guid id);
        Task<ConsentPolicy> CreateAsync(ConsentPolicy consentPolicy);
        Task<ConsentPolicy?> UpdateAsync(Guid id, ConsentPolicy consentPolicy);
        Task<bool> DeleteAsync(Guid id);
        Task<ConsentPolicy?> GetActivePolicyAsync();
        Task<IEnumerable<ConsentPolicy>> GetActiveMandatoryPoliciesAsync();
    }
}
