using System.ComponentModel.DataAnnotations;
using System.Text;
using AuthGDPR.Api.Controller.Base;
using AuthGDPR.Application;
using AuthGDPR.Application.DTOs.Account;
using AuthGDPR.Application.Interfaces;
using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Domain.Entities.Consent;
using AuthGDPR.Domain.Enums;
using AuthGDPR.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthGDPR.Api.Controller.Account
{
    // Controller che gestisce le API per login, refresh e logout
    [ApiController]
    [Route("api/account")]
    public class AccountController : ApiBaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly PseudonymizerService _pseudonymizerService;

        private readonly IOtpChallengeService _otpChallengeService;

        private readonly IAccountService _accountService;

        //private readonly IAuditLogService _auditLogService;
        private readonly IEmailCustomSender _emailCustomSender; // Servizio per l'invio delle email

        private readonly IUserConsentService _userConsentService;
        private readonly IConsentPolicyService _consentPolicyService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            PseudonymizerService pseudonymizerService,
            RefreshTokenService refreshTokenService,
            IOtpChallengeService otpChallengeService,
            IAccountService accountService,
            IAuditLogService auditLogService,
            IEmailCustomSender emailCustomSender,
            IUserConsentService userConsentService,
            IConsentPolicyService consentPolicyService) : base(auditLogService)
        {
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
            _pseudonymizerService = pseudonymizerService;

            _otpChallengeService = otpChallengeService;

            _accountService = accountService;

            //_auditLogService = auditLogService;
            _emailCustomSender = emailCustomSender;

            _userConsentService = userConsentService;
            _consentPolicyService = consentPolicyService;
        }

        // POST: /api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Application.DTOs.Account.RegisterRequest request)
        {

            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName))
            {

                // Utilizziamo il metodo helper per loggare l'evento e restituire l'errore
                return await CreateErrorResponseAsync(
                    statusCode: StatusCodes.Status400BadRequest,
                    messageCategory: MessageCategory.Errore,
                    actionType: ActionType.Register,
                    userId: Guid.Empty,
                    entityName: "ApplicationUser",
                    entityId: "0",
                    errorEnum: ApiMessages.ErroreInputDati,
                    ipAddress: HttpContext.Connection.RemoteIpAddress.ToString()
                );

                // Esegui il logging e costruisci la risposta di errore tramite la factory,
                // passando il servizio di log come parametro.
                //return ErrorResponseFactory.CreateErrorResponse(
                //    auditLogService: _auditLogService,
                //    statusCode: StatusCodes.Status400BadRequest,
                //    errorEnum: ErrorMessages.DatiMancanti,
                //    actionType: ActionType.Register,
                //    userId: Guid.Empty,
                //    entityName: "ApplicationUser",
                //    entityId: "0",
                //    ipAddress: ""
                //);
                //return BadRequest("Dati di registrazione mancanti o non validi.");
            }

            // Verifica se esiste già un utente con lo stesso username o email
            var existingUserByName = await _userManager.FindByNameAsync(request.Username);
            if (existingUserByName != null)
            {
                return BadRequest("Username già in uso.");
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest("Email già in uso.");
            }

            // Controlla che i consensi obbligatori siano accettati.
            // Recupera le policy obbligatorie attive
            var mandatoryPolicies = await _consentPolicyService.GetActiveMandatoryPoliciesAsync();
            foreach (var policy in mandatoryPolicies)
            {
                // Controlla se la lista di consensi del client contiene questo policy e se Accepted==true
                var matchingConsent = request.Consents.FirstOrDefault(c => c.ConsentPolicyId == policy.Id);
                if (matchingConsent == null || !matchingConsent.Accepted)
                {
                    return BadRequest($"La policy obbligatoria '{policy.Description}' non è stata accettata.");
                }
            }

            // Creo un id senza lasciarlo fare ad Entity framework
            // E per successivamente portelo "nasconderlo" ulteriormente
            // Incremento della sicurezza, rispetto della conformità e chiarezza sulla gestione dei dati
            Guid realUserId = Guid.NewGuid();
            Guid pseudonymizedUserId = _pseudonymizerService.GetPseudonymizedUserId(realUserId);

            // Crea il nuovo utente valorizzando i campi addizionali
            var newUser = new ApplicationUser
            {
                Id = realUserId,
                PseudonymizedUserId = pseudonymizedUserId,
                UserName = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false  // Di default, l'email non è confermata
            };

            // Si crea l'utente; il custom Argon2PasswordHasher viene usato internamente per hashare la password.
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest($"Errore nella registrazione dell'utente: {errors}");
            }

            // Dopo la creazione dell'utente, per ciascun consenso inviato nel DTO,
            // crea un record di UserConsent
            foreach (var consentDto in request.Consents)
            {
                // Se il consenso è stato espresso, anche se facoltativo, creane il record
                if (consentDto.Accepted)
                {
                    var userConsent = new UserConsent
                    {
                        UserId = realUserId,
                        ConsentPolicyId = consentDto.ConsentPolicyId,
                        ConsentType = consentDto.ConsentType,
                        IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString()
                    };

                    await _userConsentService.CreateAsync(userConsent);
                }
            }

            // Genera il token di conferma email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            // Codifica il token per l'uso in URL
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Genera l'URL basato sulla route nominata "ConfirmEmailRoute"
            var confirmationLink = Url.RouteUrl("ConfirmEmailRoute",
                new { userId = pseudonymizedUserId, token = encodedToken },
                Request.Scheme);

            // Invia l'email all'utente con il link di conferma
            await _emailCustomSender.SendEmailAsync(newUser.Email, "Conferma il tuo account",
                $"Per confermare il tuo account, <a href='{confirmationLink}'>clicca qui</a>.");

            // Recupera la policy attiva per il consenso
            //var activePolicy = await _consentPolicyService.GetActivePolicyAsync();
            //if (activePolicy == null)
            //{
            //    return BadRequest("Nessuna policy attiva disponibile per il consenso.");
            //}

            // (Opzionale) Puoi registrare un evento di audit o inviare una email di conferma.
            //await _auditLogService.LogEventAsync(
            //    userId: newUser.Id,
            //    messageCategory: MessageCategory.Successo,
            //    actionType: ActionType.Register,      // Assicurati che ActionType.Register esista nella tua enum
            //    entityName: "ApplicationUser",         // Nome dell'entità interessata
            //    entityId: newUser.Id.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
            //    description: $"L'utente '{newUser.Email}' è stato creato con successo. Link di conferma: {confirmationLink}",
            //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
            //);

            // Ritorna una risposta di successo
            return Ok(new RegisterResponse { Message = $"Registrazione avvenuta con successo. Link di conferma: {confirmationLink}" });
        }

        // GET: /api/account/confirm-email?userId=...&token=...
        [HttpGet("confirm-email", Name = "ConfirmEmailRoute")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
                return BadRequest("Parametri mancanti per la conferma dell'email.");

            var user = await _accountService.FindByPseudonymizedIdAsync(userId);
            //var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return BadRequest("Utente non trovato.");

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            var confirmResult = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (confirmResult.Succeeded)
            {
                //await _auditLogService.LogEventAsync(
                //    userId: userId,
                //    messageCategory: MessageCategory.Successo,
                //    actionType: ActionType.ConfirmEmail,      // Assicurati che ActionType.Register esista nella tua enum
                //    entityName: "ApplicationUser",         // Nome dell'entità interessata
                //    entityId: userId.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
                //    description: $"Procudura per l'utente '{userId}' effettuata con successo. Email verificata.",
                //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
                //);

                return Ok("Email confermata con successo.");
            }
            else
            {
                //await _auditLogService.LogEventAsync(
                //    userId: userId,
                //    messageCategory: MessageCategory.Errore,
                //    actionType: ActionType.Register,      // Assicurati che ActionType.Register esista nella tua enum
                //    entityName: "ApplicationUser",         // Nome dell'entità interessata
                //    entityId: userId.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
                //    description: $"Errore durante la procedura di verifica email per l'utente '{userId}'.",
                //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
                //);

                return BadRequest("Errore durante la conferma dell'email.");
            }

        }

        // POST: /api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Application.DTOs.Account.LoginRequest request)
        {
            // Verifica che la richiesta non sia nulla e che contenga le credenziali necessarie
            if (request == null ||
                string.IsNullOrWhiteSpace(request.UsernameOrEmail) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Credenziali mancanti.");
            }

            ApplicationUser user = null;

            // Se il parametro contiene '@' lo consideriamo come email, altrimenti come username
            if (request.UsernameOrEmail.Contains("@"))
                user = await _userManager.FindByEmailAsync(request.UsernameOrEmail);
            else
                user = await _userManager.FindByNameAsync(request.UsernameOrEmail);

            // Se l'utente non viene trovato o la password non corrisponde, ritorna Unauthorized
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized("Credenziali non valide.");
            }

            // Verifica che l'email sia stata confermata (utile anche per chi ha effettuato il login tramite username)
            if (!user.EmailConfirmed)
            {
                return Unauthorized("Email non confermata. Controlla la tua casella di posta per confermare il tuo account.");
            }

            // --------------

            // Invece di emettere token direttamente, creiamo il challenge 2FA
            var challenge = await _otpChallengeService.CreateChallengeAsync(user.Id);

            // Invia l'OTP via email
            var subject = "Il tuo codice OTP per l'autenticazione a due fattori";
            var htmlMessage = $"<p>Ciao, il tuo codice OTP è: <strong>{challenge.Otp}</strong></p>" +
                              $"<p>Il codice è valido per 5 minuti. Hai un massimo di 3 tentativi.</p>";
            await _emailCustomSender.SendEmailAsync(user.Email, subject, htmlMessage);

            // Restituisci al client il challengeId (necessario per la verifica dell'OTP)
            return Ok(new TwoFactorChallengeResponseDto
            {
                ChallengeId = challenge.ChallengeId,
                Message = $"Codice OTP inviato via email. Utilizza il ChallengeId per la verifica. {challenge.Otp}"
            });

            // ----------

            // Genera l'Access Token (15 minuti) e il Refresh Token (30 giorni)
            //var accessToken = _refreshTokenService.GenerateAccessToken(user);
            //var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user);

            //// Restituisce entrambi i token al client
            //return Ok(new LoginResponse
            //{
            //    AccessToken = accessToken,
            //    RefreshToken = refreshToken
            //});
        }

        // POST: /api/account/verify-otp
        [HttpPost("verify-otp", Name = "VerifyOTPRoute")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.ChallengeId) ||
                string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest("Dati mancanti per la verifica dell'OTP.");
            }

            // Verifica l'OTP e recupera il challenge se la validazione è andata a buon fine
            var challenge = await _otpChallengeService.ValidateOtpAsync(request.ChallengeId, request.Otp);
            if (challenge == null)
            {
                return Unauthorized("OTP non valido o numero di tentativi superato. Effettua nuovamente il login per richiedere un nuovo OTP.");
            }

            // Utilizza il UserId presente nel challenge per recuperare l'utente
            var user = await _userManager.FindByIdAsync(challenge.UserId.ToString());
            if (user == null)
            {
                return Unauthorized("Utente non trovato.");
            }

            // A questo punto, l'autenticazione 2FA è completata.
            // Genera il token di accesso e il refresh token
            var accessToken = _refreshTokenService.GenerateAccessToken(user.PseudonymizedUserId);
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.PseudonymizedUserId, user.Id);

            return Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }


        // POST: /api/account/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] Application.DTOs.Account.RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token mancante.");

            // Valida il refresh token lato JWT e nel database
            var validationResult = await _refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken);
            if (!validationResult.isValid)
                return Unauthorized("Refresh token non valido o scaduto.");

            // Estrae il claim pseudonimizzato ("uid")
            var pseudoUserId = validationResult.principal.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(pseudoUserId))
                return Unauthorized("Token privo di identificatore utente.");

            // Recupera l'utente utilizzando il metodo helper che calcola lo stesso hash
            Guid pseudoUserIdGuid = Guid.Parse(pseudoUserId);
            var user = await _pseudonymizerService.GetUserByPseudonymizedIdAsync(pseudoUserIdGuid);
            if (user == null)
                return Unauthorized("Utente non trovato.");

            // Ruota il refresh token (revoca quello corrente e ne crea uno nuovo)
            var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(request.RefreshToken, user.PseudonymizedUserId, user.Id);
            //var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(request.RefreshToken, user);
            // Genera un nuovo Access Token
            var newAccessToken = _refreshTokenService.GenerateAccessToken(user.PseudonymizedUserId);
            //var newAccessToken = _refreshTokenService.GenerateAccessToken(user);

            // Registra l'evento di login (audit logging)
            //await _auditLogService.LogEventAsync(
            //    userId: user.Id,
            //    messageCategory: MessageCategory.Successo,
            //    actionType: ActionType.Refresh,      // Assicurati che ActionType.Register esista nella tua enum
            //    entityName: "RefreskToken",         // Nome dell'entità interessata
            //    entityId: user.Id.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
            //    description: $"Procudura per l'utente '{user.Id}' effettuata con successo. Refresh token effettuato.",
            //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
            //);

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        // POST: /api/account/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token mancante.");

            // Valida il refresh token
            var validationResult = await _refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken);
            if (validationResult.isValid)
            {
                var tokenId = validationResult.principal.FindFirst("tid")?.Value;
                var pseudoUserId = validationResult.principal.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(tokenId) && !string.IsNullOrEmpty(pseudoUserId))
                {
                    Guid pseudoUserIdGuid = Guid.Parse(pseudoUserId);
                    var user = await _pseudonymizerService.GetUserByPseudonymizedIdAsync(pseudoUserIdGuid);
                    if (user != null)
                    {
                        await _refreshTokenService.RevokeRefreshTokenAsync(tokenId, user.Id);

                        // Registra l'evento di login (audit logging)
                        //await _auditLogService.LogEventAsync(
                        //    userId: user.Id,
                        //    messageCategory: MessageCategory.Successo,
                        //    actionType: ActionType.Logout,      // Assicurati che ActionType.Register esista nella tua enum
                        //    entityName: "RefreskToken",         // Nome dell'entità interessata
                        //    entityId: user.Id.ToString(),       // L'ID dell'entità (in questo caso, l'utente)
                        //    description: $"Procudura per l'utente '{user.Id}' effettuata con successo. Logout effettuato.",
                        //    ipAddress: null                        // Oppure, se disponibile, l'indirizzo IP
                        //);

                        return Ok("Logout effettuato con successo.");
                    }
                }
            }
            return BadRequest("Refresh token non valido.");
        }

        /// <summary>
        /// Ricostruisce l'utente reale a partire dal valore pseudonimizzato.
        /// In alternativa, potresti memorizzare il valore pseudonimizzato direttamente nel record utente.
        /// </summary>
        //private async Task<ApplicationUser> GetUserByPseudonymizedIdAsync(Guid pseudoUserId)
        //{
        //    // Itera sugli utenti e confronta l'hash calcolato usando il TokenHelper
        //    foreach (var user in _userManager.Users)
        //    {
        //        var computed = _pseudonymizerService.GetPseudonymizedUserId(user.Id);
        //        if (computed == pseudoUserId)
        //            return user;
        //    }
        //    return null;
        //}

    }
}
