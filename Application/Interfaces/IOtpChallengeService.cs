using AuthGDPR.Application.DTOs.Account;

namespace AuthGDPR.Application.Interfaces
{
    public interface IOtpChallengeService
    {
        Task<TwoFactorChallenge> CreateChallengeAsync(Guid userId);
        Task<TwoFactorChallenge> GetChallengeAsync(string challengeId);
        /// <summary>
        /// Se l’OTP è corretto ritorna il challenge (contenente almeno il UserId) e rimuove il record dalla cache;
        /// altrimenti aggiorna il conteggio dei tentativi e ritorna null.
        /// </summary>
        Task<TwoFactorChallenge> ValidateOtpAsync(string challengeId, string otp);
        Task RemoveChallengeAsync(string challengeId);
    }

}
