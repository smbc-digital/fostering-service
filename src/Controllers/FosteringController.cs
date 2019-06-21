using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using fostering_service.Services;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;

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

        [Route("about-yourself")]
        [HttpPatch]
        public async Task<IActionResult> UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model)
        {
            try
            {
                var response = await _fosteringService.UpdateAboutYourself(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("your-employment-details")]
        [HttpPatch]
        public async Task<IActionResult> UpdateYourEmploymentDetails(FosteringCaseYourEmploymentDetailsUpdateModel model)
        {
            try
            {
                await _fosteringService.UpdateYourEmploymentDetails(model);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("update-form-status")]
        [HttpPatch]
        public async Task<IActionResult> UpdateFormStatus(FosteringCaseStatusUpdateModel model)
        {
            try
            {
                await _fosteringService.UpdateStatus(model.CaseId, model.Status, model.Form);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}