using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using fostering_service.Attributes;
using fostering_service.Services.HomeVisit;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;
using StockportGovUK.NetStandard.Models.Fostering.HomeVisit;
using StockportGovUK.NetStandard.Models;

namespace fostering_service.Controllers.HomeVisit
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    [TokenAuthentication]
    public class HomeVisitController : ControllerBase
    {
        private readonly IHomeVisitService _homeVisitService;
        private readonly ILogger<HomeVisitController> _logger;

        public HomeVisitController(IHomeVisitService homeVisitService, ILogger<HomeVisitController> logger)
        {
            _homeVisitService = homeVisitService;
            _logger = logger;
        }

        [Route("status")]
        [HttpPatch]
        public async Task<IActionResult> UpdateFormStatus(FosteringCaseStatusUpdateModel model)
        {
            try
            {
                await _homeVisitService.UpdateStatus(model.CaseId, model.Status, model.Form);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("about-yourself")]
        [HttpPatch]
        [BlockHomeVisitUpdate]
        public async Task<IActionResult> UpdateAboutYourself(FosteringCaseAboutYourselfUpdateModel model)
        {
            try
            {
                var response = await _homeVisitService.UpdateAboutYourself(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"FosteringController UpdateAboutYourself, an error has occured while attempting to call fostering service, ex: {ex}");
                return StatusCode(500, ex);
            }
        }

        [Route("your-employment-details")]
        [HttpPatch]
        [BlockHomeVisitUpdate]
        public async Task<IActionResult> UpdateYourEmploymentDetails(FosteringCaseYourEmploymentDetailsUpdateModel model)
        {
            try
            {
                var response = await _homeVisitService.UpdateYourEmploymentDetails(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("languages-spoken-in-your-home")]
        [HttpPatch]
        [BlockHomeVisitUpdate]
        public async Task<IActionResult> UpdateLanguagesSpokenInYourHome(FosteringCaseLanguagesSpokenInYourHomeUpdateModel model)
        {
            try
            {
                var response = await _homeVisitService.UpdateLanguagesSpokenInYourHome(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("health-status")]
        [HttpPatch]
        [BlockHomeVisitUpdate]
        public async Task<IActionResult> UpdateHealthStatus(FosteringCaseHealthUpdateModel model)
        {
            try
            {
                var response = await _homeVisitService.UpdateHealthStatus(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("update-household")]
        [HttpPatch]
        [BlockHomeVisitUpdate]
        public async Task<IActionResult> UpdateHousehold(FosteringCaseHouseholdUpdateModel model)
        {
            try
            {
                var response = await _homeVisitService.UpdateHousehold(model);

                return Ok(response);
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
                var response = await _homeVisitService.UpdatePartnershipStatus(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("your-fostering-history")]
        [HttpPatch]
        [BlockHomeVisitUpdate]
        public async Task<IActionResult> UpdateYourFosteringHistory(FosteringCaseYourFosteringHistoryUpdateModel model)
        {
            try
            {
                var response = await _homeVisitService.UpdateYourFosteringHistory(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("interest-in-fostering")]
        [HttpPatch]
        [BlockHomeVisitUpdate]
        public async Task<IActionResult> UpdateInterestInFostering(FosteringCaseInterestInFosteringUpdateModel model)
        {
            try
            {
                var response = await _homeVisitService.UpdateInterestInFostering(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [Route("children-living-away-from-home")]
        [HttpPatch]
        [BlockHomeVisitUpdate]
        public async Task<IActionResult> UpdateChildrenLivingAwayFromHome(FosteringCaseChildrenLivingAwayFromHomeUpdateModel model)
        {
            try
            {
                var response = await _homeVisitService.UpdateChildrenLivingAwayFromHome(model);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }     
    }
}