using System;
using System.Threading.Tasks;
using fostering_service.Services.Case;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;

namespace fostering_service.Controllers.Case
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    [TokenAuthentication]
    public class CaseController : ControllerBase
    {
        private readonly ICaseService _caseService;
        private readonly ILogger<CaseController> _logger;

        public CaseController(ICaseService caseService, ILogger<CaseController> logger)
        {
            _caseService = caseService;
            _logger = logger;
        }

        [Route("case")]
        [HttpGet]
        public async Task<IActionResult> GetCase([FromQuery]string caseId)
        {
            try
            {
                var result = await _caseService.GetCase(caseId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"CaseController GetCase an exception has occured while calling fostering sevice getCase, ex: {ex}");
                return StatusCode(500, ex);
            }
        }
    }
}
