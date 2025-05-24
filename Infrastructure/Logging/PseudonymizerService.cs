using AuthGDPR.Domain;
using AuthGDPR.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace AuthGDPR.Infrastructure.Logging
{
    public class PseudonymizationOptions
    {
        public string Salt { get; set; }
    }

    public class PseudonymizerService
    {
        private readonly string _salt;

        private readonly UserManager<ApplicationUser> _userManager;

        public PseudonymizerService(IOptions<PseudonymizationOptions> options, UserManager<ApplicationUser> userManager)
        {
            _salt = options.Value.Salt;
            _userManager = userManager;
        }

        public string Pseudonymize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            using (var sha256 = SHA256.Create())
            {
                // Combina il salt con l’input
                var combined = _salt + input;
                var bytes = Encoding.UTF8.GetBytes(combined);
                var hash = sha256.ComputeHash(bytes);

                // Restituisce l’hash in formato esadecimale
                var sb = new StringBuilder();
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public Guid GetPseudonymizedUserId(Guid userId)
        {
            // Genera l'hash in formato esadecimale (64 caratteri)
            string hashString = Pseudonymize(userId.ToString());

            // Estrai i primi 32 caratteri (ciascuno rappresentante 4 bit, totali 128 bit)
            string guidString = hashString.Substring(0, 32);

            // Converte la stringa esadecimale in un Guid
            return Guid.ParseExact(guidString, "N");
        }
        //public string GetPseudonymizedUserId(Guid userId)
        //{
        //    return Pseudonymize(userId.ToString());
        //}

        /// <summary>
        /// Ricostruisce l'utente reale a partire dal valore pseudonimizzato.
        /// In alternativa, potresti memorizzare il valore pseudonimizzato direttamente nel record utente.
        /// </summary>
        public async Task<ApplicationUser> GetUserByPseudonymizedIdAsync(Guid pseudoUserId)
        {
            // Itera sugli utenti e confronta l'hash calcolato usando il TokenHelper
            foreach (var user in _userManager.Users)
            {
                var computed = GetPseudonymizedUserId(user.Id);
                if (computed == pseudoUserId)
                    return user;
            }
            return null;
        }
    }
}
