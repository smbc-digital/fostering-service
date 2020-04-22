using System;
using System.Threading.Tasks;
using fostering_service.Attributes;
using fostering_service.Services.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;
using StockportGovUK.NetStandard.Models.Fostering.Application;

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

        [Route("status")]
        [HttpPatch]
        [BlockApplicationUpdate(CaseReferencePropertyName = "CaseId")]
        public async Task<IActionResult> UpdateStatus(FosteringCaseStatusUpdateModel model)
        {
            try
            {
                await _applicationService.UpdateStatus(model.CaseId, model.Status, model.Form);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("gp-details")]
        [HttpPatch]
        [BlockApplicationUpdate]
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
        [BlockApplicationUpdate]
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

        [Route("councillors-details")]
        [HttpPatch]
        [BlockApplicationUpdate]
        public async Task<IActionResult> UpdateCouncillorsDetails(FosteringCaseCouncillorsUpdateModel model)
        {
            try
            {
                var response = await _applicationService.UpdateCouncillorsDetails(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("address-history")]
        [HttpPatch]
        [BlockApplicationUpdate]
        public async Task<IActionResult> UpdateAddressHistory(FosteringCaseAddressHistoryUpdateModel model)
        {
            try
            {
                var response = await _applicationService.UpdateAddressHistory(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}
