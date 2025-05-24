using AuthGDPR.Domain.Entities.Auth;

namespace AuthGDPR.Application.Interfaces
{
    public interface IAccountService
    {
        Task<ApplicationUser?> FindByPseudonymizedIdAsync(Guid pseudonymizedUserId);
    }
}
