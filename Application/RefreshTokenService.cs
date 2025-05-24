using AuthGDPR.Domain.Entities.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthGDPR.Infrastructure.Persistance;
using Microsoft.IdentityModel.Tokens;
using AuthGDPR.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using AuthGDPR.Domain.Enums;

namespace AuthGDPR.Application
{
    public class RefreshTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly PseudonymizerService _pseudonymizerService;
        private readonly IAuditLogService _auditLogService;

        public RefreshTokenService(IConfiguration configuration,
                                   AppDbContext dbContext,
                                   PseudonymizerService pseudonymizerService,
                                   IAuditLogService auditLogService)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _pseudonymizerService = pseudonymizerService;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Genera l’Access Token (JWT) per l’utente passato.
        /// Il token ha una durata di 10 minuti e viene firmato con l'algoritmo HS256.
        /// </summary>
        /// <param name="user">Istanza di ApplicationUser</param>
        /// <returns>Una stringa contenente il JWT</returns>
        public string GenerateAccessToken(Guid peudonymizedUserId)
        {
            // Definizione della scadenza del token (10 minuti)
            var expiration = DateTime.UtcNow.AddMinutes(10);

            // Costruisco i claim, includendo l'ID pseudonimizzato e altri claim necessari
            var claims = new List<Claim>
            {
                // Claim custom per l’ID dell’utente in forma pseudonimizzata
                //new Claim("uid", _pseudonymizerService.GetPseudonymizedUserId(user.Id).ToString()),
                new Claim("uid", peudonymizedUserId.ToString()),
                // Claim IAT: issued at
                new Claim(JwtRegisteredClaimNames.Iat,
                          new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                          ClaimValueTypes.Integer64)
            };

            // È buona prassi includere anche un identificativo univoco per il token (JTI)
            var tokenId = Guid.NewGuid().ToString();
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, tokenId));

            // Prelevo la chiave segreta dalla configurazione per firmare il token (sezione "Jwt:Key")
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Creo il token specificando issuer, audience, claim, scadenza e le credenziali di firma
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],      // es. https://localhost:7040
                audience: _configuration["Jwt:Audience"],    // es. gdpr-api
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            // Serializzo il token in stringa e lo restituisco
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        //public string GenerateAccessToken(ApplicationUser user)
        //{
        //    // Definizione della scadenza del token (10 minuti)
        //    var expiration = DateTime.UtcNow.AddMinutes(10);

        //    // Costruisco i claim, includendo l'ID pseudonimizzato e altri claim necessari
        //    var claims = new List<Claim>
        //    {
        //        // Claim custom per l’ID dell’utente in forma pseudonimizzata
        //        //new Claim("uid", _pseudonymizerService.GetPseudonymizedUserId(user.Id).ToString()),
        //        new Claim("uid", user.PseudonymizedUserId.ToString()),
        //        // Claim IAT: issued at
        //        new Claim(JwtRegisteredClaimNames.Iat,
        //                  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
        //                  ClaimValueTypes.Integer64)
        //    };

        //    // È buona prassi includere anche un identificativo univoco per il token (JTI)
        //    var tokenId = Guid.NewGuid().ToString();
        //    claims.Add(new Claim(JwtRegisteredClaimNames.Jti, tokenId));

        //    // Prelevo la chiave segreta dalla configurazione per firmare il token (sezione "Jwt:Key")
        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    // Creo il token specificando issuer, audience, claim, scadenza e le credenziali di firma
        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],      // es. https://localhost:7040
        //        audience: _configuration["Jwt:Audience"],    // es. gdpr-api
        //        claims: claims,
        //        expires: expiration,
        //        signingCredentials: signingCredentials
        //    );

        //    // Serializzo il token in stringa e lo restituisco
        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        /// <summary>
        /// Crea un nuovo Refresh Token, lo firma con HS256 e lo salva nel database.
        /// Il token ha durata di 30 giorni.
        /// </summary>
        public async Task<string> CreateRefreshTokenAsync(Guid peudonymizedUserId, Guid realUserId)
        {
            var expiration = DateTime.UtcNow.AddDays(30);

            // Genera un ID univoco per il token (claim "tid")
            var tokenId = Guid.NewGuid().ToString();

            // I claim includono il tokenId e l'ID utente pseudonimizzato (claim "uid")
            var claims = new List<Claim>
            {
                new Claim("tid", tokenId),
                //new Claim("uid", _pseudonymizerService.GetPseudonymizedUserId(user.Id).ToString()),
                new Claim("uid", peudonymizedUserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Recupera la chiave per i refresh token dalla sezione "Jwt:RefreshKey" in appsettings.json.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:RefreshKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Costruisce il token JWT
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Salva nel database il refresh token per consentirne la validazione e la rotazione
            await SaveRefreshTokenAsync(tokenId, realUserId, DateTime.UtcNow, expiration);

            return tokenString;
        }
        //public async Task<string> CreateRefreshTokenAsync(ApplicationUser user)
        //{
        //    var expiration = DateTime.UtcNow.AddDays(30);

        //    // Genera un ID univoco per il token (claim "tid")
        //    var tokenId = Guid.NewGuid().ToString();

        //    // I claim includono il tokenId e l'ID utente pseudonimizzato (claim "uid")
        //    var claims = new List<Claim>
        //    {
        //        new Claim("tid", tokenId),
        //        //new Claim("uid", _pseudonymizerService.GetPseudonymizedUserId(user.Id).ToString()),
        //        new Claim("uid", user.PseudonymizedUserId.ToString()),
        //        new Claim(JwtRegisteredClaimNames.Iat,
        //            new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        //    };

        //    // Recupera la chiave per i refresh token dalla sezione "Jwt:RefreshKey" in appsettings.json.
        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:RefreshKey"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    // Costruisce il token JWT
        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: expiration,
        //        signingCredentials: creds
        //    );

        //    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        //    // Salva nel database il refresh token per consentirne la validazione e la rotazione
        //    await SaveRefreshTokenAsync(tokenId, user.Id, DateTime.UtcNow, expiration);

        //    // Registra l'evento di creazione del token

        //    // Registra l'evento di login (audit logging)
        //    //await _auditLogService.LogEventAsync(
        //    //    userId: user.Id,
        //    //    actionType: ActionType.Logout,      // Assicurati che ActionType.Register esista nella tua enum
        //    //    entityName: "RefreskToken",         // Nome dell'entità interessata
        //    //    entityId: user.Id.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
        //    //    description: $"Procudura per l'utente '{user.Id}' effettuata con successo. Logout effettuato.",
        //    //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
        //    //);

        //    return tokenString;
        //}

        // Salva il refresh token nella tabella custom.
        private async Task SaveRefreshTokenAsync(string tokenId, Guid userId, DateTime issuedAt, DateTime expiresAt)
        {
            var refreshTokenEntity = new RefreshTokenEntity
            {
                TokenId = tokenId,
                UserId = userId,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt,
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            // Logga l'evento usando l'ID pseudonimizzato (viene gestito internamente nel servizio)

            // Registra l'evento di login (audit logging)
            //await _auditLogService.LogEventAsync(
            //    userId: user.Id,
            //    actionType: ActionType.Logout,      // Assicurati che ActionType.Register esista nella tua enum
            //    entityName: "RefreskToken",         // Nome dell'entità interessata
            //    entityId: user.Id.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
            //    description: $"Procudura per l'utente '{user.Id}' effettuata con successo. Logout effettuato.",
            //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
            //);
        }

        /// <summary>
        /// Valida un refresh token:
        /// - Controlla la firma, l’issuer, l’audience e l’expiration.
        /// - Verifica inoltre che il token sia presente nel DB e non sia stato revocato.
        /// </summary>
        public async Task<(bool isValid, ClaimsPrincipal principal)> ValidateRefreshTokenAsync(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:RefreshKey"])),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
                // Verifica che il token sia presente e valido (non revocato) nel DB
                var tokenId = principal.FindFirst("tid")?.Value;
                if (string.IsNullOrEmpty(tokenId) || !await IsRefreshTokenValidAsync(tokenId))
                {
                    return (false, null);
                }

                return (true, principal);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning("Validazione del Refresh Token fallita: {Message}", ex.Message);
                return (false, null);
            }
        }

        /// <summary>
        /// Esegue la rotazione del refresh token:
        /// valida quello corrente, lo revoca e ne genera uno nuovo.
        /// </summary>
        public async Task<string> RotateRefreshTokenAsync(string currentRefreshToken, Guid peudonymizedUserId, Guid realUserId)
        {
            var validationResult = await ValidateRefreshTokenAsync(currentRefreshToken);
            if (!validationResult.isValid)
            {
                throw new SecurityTokenException("Refresh Token non valido");
            }

            var tokenId = validationResult.principal.FindFirst("tid")?.Value;
            if (string.IsNullOrEmpty(tokenId))
            {
                throw new SecurityTokenException("Refresh Token privo di identificatore");
            }

            // Revoca il token corrente
            await RevokeRefreshTokenAsync(tokenId, realUserId);

            // Crea e restituisce un nuovo refresh token
            var newRefreshToken = await CreateRefreshTokenAsync(peudonymizedUserId, realUserId);

            // Registra l'evento di login (audit logging)
            //await _auditLogService.LogEventAsync(
            //    userId: user.Id,
            //    actionType: ActionType.Logout,      // Assicurati che ActionType.Register esista nella tua enum
            //    entityName: "RefreskToken",         // Nome dell'entità interessata
            //    entityId: user.Id.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
            //    description: $"Procudura per l'utente '{user.Id}' effettuata con successo. Logout effettuato.",
            //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
            //);

            return newRefreshToken;
        }
        //public async Task<string> RotateRefreshTokenAsync(string currentRefreshToken, ApplicationUser user)
        //{
        //    var validationResult = await ValidateRefreshTokenAsync(currentRefreshToken);
        //    if (!validationResult.isValid)
        //    {
        //        throw new SecurityTokenException("Refresh Token non valido");
        //    }

        //    var tokenId = validationResult.principal.FindFirst("tid")?.Value;
        //    if (string.IsNullOrEmpty(tokenId))
        //    {
        //        throw new SecurityTokenException("Refresh Token privo di identificatore");
        //    }

        //    // Revoca il token corrente
        //    await RevokeRefreshTokenAsync(tokenId, user.Id);

        //    // Crea e restituisce un nuovo refresh token
        //    var newRefreshToken = await CreateRefreshTokenAsync(user);

        //    // Registra l'evento di login (audit logging)
        //    //await _auditLogService.LogEventAsync(
        //    //    userId: user.Id,
        //    //    actionType: ActionType.Logout,      // Assicurati che ActionType.Register esista nella tua enum
        //    //    entityName: "RefreskToken",         // Nome dell'entità interessata
        //    //    entityId: user.Id.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
        //    //    description: $"Procudura per l'utente '{user.Id}' effettuata con successo. Logout effettuato.",
        //    //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
        //    //);

        //    return newRefreshToken;
        //}

        /// <summary>
        /// Revoca un refresh token impostandone lo stato nel DB.
        /// Questo metodo verrà richiamato per il logout o in caso di attività sospette.
        /// </summary>
        public async Task RevokeRefreshTokenAsync(string tokenId, Guid userId)
        {
            var refreshToken = await _dbContext.RefreshTokens
                                    .FirstOrDefaultAsync(rt => rt.TokenId == tokenId && rt.UserId == userId);

            if (refreshToken != null && !refreshToken.IsRevoked)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();


                // Registra l'evento di login (audit logging)
                //await _auditLogService.LogEventAsync(
                //    userId: user.Id,
                //    actionType: ActionType.Logout,      // Assicurati che ActionType.Register esista nella tua enum
                //    entityName: "RefreskToken",         // Nome dell'entità interessata
                //    entityId: user.Id.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
                //    description: $"Procudura per l'utente '{user.Id}' effettuata con successo. Logout effettuato.",
                //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
                //);
            }
        }

        // Verifica che il token esista, non sia revocato e non sia scaduto
        private async Task<bool> IsRefreshTokenValidAsync(string tokenId)
        {
            var tokenEntity = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenId == tokenId);
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt <= DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }
    }
}
