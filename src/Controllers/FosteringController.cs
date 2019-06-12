using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using fostering_service.Services;

namespace fostering_service.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class FosteringController : ControllerBase
    {
        private readonly IFosteringService _fosteringService;

        public FosteringController(IFosteringService fosteringService)
        {
            _fosteringService = fosteringService;
        }

        [Route("case")]
        [HttpGet]
        public async Task<IActionResult> GetCase([FromQuery]string caseId)
        {
            try
            {
                var result = await _fosteringService.GetCase(caseId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}