using AuthGDPR.Api.Controller.Base;
using AuthGDPR.Application.DTOs.Consent;
using AuthGDPR.Application.Interfaces;
using AuthGDPR.Domain.Entities.Consent;
using AuthGDPR.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthGDPR.Api.Controller.Consent
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserConsentsController : ApiBaseController
    {
        private readonly IUserConsentService _userConsentService;
        //private readonly IAuditLogService _auditLogService;

        public UserConsentsController(IUserConsentService userConsentService, IAuditLogService auditLogService)
            : base(auditLogService)
        {
            _userConsentService = userConsentService;
            //_auditLogService = auditLogService;
        }

        // GET: api/UserConsents?userId={userId}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserConsent>>> GetAll([FromQuery] Guid userId)
        {
            // In un ambiente reale l'userId deve essere estratto dal token dell'utente autenticato.
            var consents = await _userConsentService.GetAllByUserIdAsync(userId);
            return Ok(consents);
        }

        // GET: api/UserConsents/{id}?userId={userId}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserConsent>> GetById(Guid id, [FromQuery] Guid userId)
        {
            var consent = await _userConsentService.GetByIdAsync(id, userId);
            if (consent == null)
                return NotFound();
            return Ok(consent);
        }

        // POST: api/UserConsents?userId={userId}
        [HttpPost]
        public async Task<ActionResult<UserConsent>> Create([FromQuery] Guid userId, [FromBody] CreateUserConsentDto dto)
        {
            // Costruiamo l'entità UserConsent (l'userId sarà preso dal token in una situazione reale)
            var userConsent = new UserConsent
            {
                UserId = userId,
                ConsentType = dto.ConsentType,
                ConsentPolicyId = dto.ConsentPolicyId,
                IPAddress = dto.IPAddress,
                UserAgent = dto.UserAgent
            };

            var createdConsent = await _userConsentService.CreateAsync(userConsent);
            return CreatedAtAction(nameof(GetById), new { id = createdConsent.Id, userId = userId }, createdConsent);
        }

        // PUT: api/UserConsents/{id}?userId={userId}
        [HttpPut("{id}")]
        public async Task<ActionResult<UserConsent>> Update(Guid id, [FromQuery] Guid userId, [FromBody] UpdateUserConsentDto dto)
        {
            var updatedConsentEntity = new UserConsent
            {
                // Non impostiamo l'Id, che serve solo come parametro di ricerca
                UserId = userId,
                ConsentType = dto.ConsentType,
                ConsentPolicyId = dto.ConsentPolicyId,
                IPAddress = dto.IPAddress,
                UserAgent = dto.UserAgent
            };

            var result = await _userConsentService.UpdateAsync(id, updatedConsentEntity, userId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // DELETE: api/UserConsents/{id}?userId={userId}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid userId)
        {
            var deleted = await _userConsentService.DeleteAsync(id, userId);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
