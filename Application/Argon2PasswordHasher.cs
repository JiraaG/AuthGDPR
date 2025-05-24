using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Identity;

namespace AuthGDPR.Application
{
    public class Argon2PasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        public string HashPassword(TUser user, string password)
        {
            return Argon2.Hash(password); // Usa la tua libreria preferita
        }
        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            return Argon2.Verify(hashedPassword, providedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}
