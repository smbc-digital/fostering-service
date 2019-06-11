using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;

namespace fostering_service.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class FosteringController : ControllerBase
    {
        private readonly IVerintServiceGateway _verintServiceGateway;

        public FosteringController(IVerintServiceGateway verintServiceGateway)
        {
            _verintServiceGateway = verintServiceGateway;
        }

        [Route("case")]
        [HttpGet]
        public async Task<IActionResult> GetCase()
        {
            var result = await _verintServiceGateway.GetCase("101004219219");

            return Ok(result);
        }
    }
}