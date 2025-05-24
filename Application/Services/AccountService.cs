using AuthGDPR.Application.Interfaces;
using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace AuthGDPR.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> FindByPseudonymizedIdAsync(Guid pseudonymizedUserId)
        {
            return await _context.ApplicationUsers
                                 .FirstOrDefaultAsync(u => u.PseudonymizedUserId == pseudonymizedUserId);
        }
    }
}
