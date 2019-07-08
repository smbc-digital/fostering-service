using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using fostering_service.Services;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;
using StockportGovUK.NetStandard.Models.Models;
using StockportGovUK.NetStandard.Models.Models.Fostering.Update;

namespace fostering_service.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    [TokenAuthentication]
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

        [Route("languages-spoken-in-your-home")]
        [HttpPatch]
        public async Task<IActionResult> UpdateLanguagesSpokenInYourHome(FosteringCaseLanguagesSpokenInYourHomeUpdateModel model)
        {
            try
            {
                var response = await _fosteringService.UpdateLanguagesSpokenInYourHome(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("health-status")]
        [HttpPatch]
        public async Task<IActionResult> UpdateHealthStatus(FosteringCaseHealthUpdateModel model)
        {
            try
            {
                var response = await _fosteringService.UpdateHealthStatus(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("update-household")]
        [HttpPatch]
        public async Task<IActionResult> UpdateHousehold(FosteringCaseHouseholdUpdateModel model)
        {
            try
            {
                var response = await _fosteringService.UpdateHousehold(model);

                return Ok(response);
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

        [Route("partnership-status")]
        [HttpPatch]
        public async Task<IActionResult> UpdatePartnershipStatus(FosteringCasePartnershipStatusUpdateModel model)
        {
            try
            {
                var response = await _fosteringService.UpdatePartnershipStatus(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("your-fostering-history")]
        [HttpPatch]
        public async Task<IActionResult> UpdateYourFosteringHistory(FosteringCaseYourFosteringHistoryUpdateModel model)
        {
            try
            {
                var response = await _fosteringService.UpdateYourFosteringHistory(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("interest-in-fostering")]
        [HttpPatch]
        public async Task<IActionResult> UpdateInterestInFostering(FosteringCaseInterestInFosteringUpdateModel model)
        {
            try
            {
                var response = await _fosteringService.UpdateInterestInFostering(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}