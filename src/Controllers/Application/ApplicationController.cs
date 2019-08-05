using System;
using System.Threading.Tasks;
using fostering_service.Services.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;

namespace fostering_service.Controllers.Application
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    [TokenAuthentication]

    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(IApplicationService applicationService, ILogger<ApplicationController> logger)
        {
            _applicationService = applicationService;
            _logger = logger;
        }

        [Route("gp-details")]
        [HttpPatch]
        public async Task<IActionResult> UpdateGpDetails(FosteringCaseGpDetailsUpdateModel model)
        {
            try
            {
                var response = await _applicationService.UpdateGpDetails(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("references")]
        [HttpPatch]
        public async Task<IActionResult> UpdateReferences(FosteringCaseReferenceUpdateModel model)
        {
            try
            {
                var response = await _applicationService.UpdateReferences(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }

}
