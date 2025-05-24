using AuthGDPR.Api.Controller.Base;
using AuthGDPR.Application.DTOs.Consent;
using AuthGDPR.Application.Interfaces;
using AuthGDPR.Domain.Entities.Consent;
using AuthGDPR.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthGDPR.Api.Controller.Consent
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsentPoliciesController : ApiBaseController
    {
        private readonly IConsentPolicyService _consentPolicyService;
        //private readonly IAuditLogService _auditLogService;

        public ConsentPoliciesController(IConsentPolicyService consentPolicyService, IAuditLogService auditLogService)
            : base(auditLogService)
        {
            _consentPolicyService = consentPolicyService;
            //_auditLogService = auditLogService;
        }

        // GET: api/ConsentPolicies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsentPolicy>>> GetAll()
        {
            var policies = await _consentPolicyService.GetAllAsync();
            return Ok(policies);
        }

        // GET: api/ConsentPolicies/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsentPolicy>> GetById(Guid id)
        {
            var policy = await _consentPolicyService.GetByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }
            return Ok(policy);
        }

        // POST: api/ConsentPolicies
        [HttpPost]
        public async Task<ActionResult<ConsentPolicy>> Create([FromBody] CreateConsentPolicyDto dto)
        {
            var policy = new ConsentPolicy
            {
                Id = Guid.NewGuid(),
                Version = dto.Version,
                Text = dto.Text,
                Description = dto.Description,
                EffectiveDate = dto.EffectiveDate,
                ConsentType = dto.ConsentType,
                IsMandatory = dto.IsMandatory,
            };

            var createdPolicy = await _consentPolicyService.CreateAsync(policy);
            return CreatedAtAction(nameof(GetById), new { id = createdPolicy.Id }, createdPolicy);
        }

        // PUT: api/ConsentPolicies/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ConsentPolicy>> Update(Guid id, [FromBody] UpdateConsentPolicyDto dto)
        {
            var existingPolicy = await _consentPolicyService.GetByIdAsync(id);
            if (existingPolicy == null)
            {
                return NotFound();
            }

            // Creiamo la nuova versione del record basandoci sui dati passati nel DTO
            var newPolicy = new ConsentPolicy
            {
                Version = dto.Version,
                Text = dto.Text,
                Description = dto.Description,
                EffectiveDate = dto.EffectiveDate,
                ConsentType = dto.ConsentType,
                IsMandatory= dto.IsMandatory,
            };

            var updatedPolicy = await _consentPolicyService.UpdateAsync(id, newPolicy);
            if (updatedPolicy == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore durante l'aggiornamento");
            }
            return Ok(updatedPolicy);
        }

        // DELETE: api/ConsentPolicies/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _consentPolicyService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
