using System.Security.Cryptography;
using System.Text;
using AuthGDPR.Application.DTOs.Account;
using AuthGDPR.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AuthGDPR.Application.Services
{
    public class OtpChallengeService : IOtpChallengeService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _challengeTTL = TimeSpan.FromMinutes(5);
        private const int MaxAttempts = 3;

        public OtpChallengeService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<TwoFactorChallenge> CreateChallengeAsync(Guid userId)
        {
            string otp = GenerateOtp();
            string hashedOtp = HashOtp(otp);

            var challengeCache = new TwoFactorChallenge
            {
                ChallengeId = Guid.NewGuid().ToString(),
                UserId = userId,
                Otp = hashedOtp,
                CreatedAt = DateTime.UtcNow,
                AttemptCount = 0
            };

            _cache.Set(challengeCache.ChallengeId, challengeCache, _challengeTTL);

            var challengeUser = new TwoFactorChallenge
            {
                ChallengeId = challengeCache.ChallengeId,
                UserId = challengeCache.UserId,
                Otp = otp,
                CreatedAt = challengeCache.CreatedAt,
                AttemptCount = challengeCache.AttemptCount,
            };

            return Task.FromResult(challengeUser);
        }

        private string GenerateOtp(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = RandomNumberGenerator.Create();
            var otpChars = new char[length];
            byte[] randomBytes = new byte[length];

            random.GetBytes(randomBytes);

            for (int i = 0; i < length; i++)
            {
                otpChars[i] = chars[randomBytes[i] % chars.Length];
            }

            return new string(otpChars);
        }

        private string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(otp);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        //private string GenerateOtp()
        //{
        //    // Genera un codice numerico a 6 cifre
        //    Random random = new Random();
        //    int otpValue = random.Next(0, 1000000);
        //    return otpValue.ToString("D6");
        //}

        public Task<TwoFactorChallenge> GetChallengeAsync(string challengeId)
        {
            _cache.TryGetValue(challengeId, out TwoFactorChallenge challenge);
            return Task.FromResult(challenge);
        }

        public async Task<TwoFactorChallenge> ValidateOtpAsync(string challengeId, string otp)
        {
            var challenge = await GetChallengeAsync(challengeId);
            if (challenge == null)
            {
                return null;
            }

            if (challenge.AttemptCount >= MaxAttempts)
            {
                await RemoveChallengeAsync(challengeId);
                return null;
            }

            // Confronta l'hash dell'OTP inserito con quello salvato
            string hashedInput = HashOtp(otp);
            if (challenge.Otp == hashedInput)
            {
                await RemoveChallengeAsync(challengeId);
                return challenge;
            }
            else
            {
                challenge.AttemptCount++;
                _cache.Set(challenge.ChallengeId, challenge, _challengeTTL);
                return null;
            }
        }

        public Task RemoveChallengeAsync(string challengeId)
        {
            _cache.Remove(challengeId);
            return Task.CompletedTask;
        }
    }

}
