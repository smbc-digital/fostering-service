using System;
using System.Linq;
using fostering_service.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Models.Models.Fostering;

namespace fostering_service.Attributes
{
    public class BlockHomeVisitUpdate : ActionFilterAttribute
    {
        public string CaseReferencePropertyName { get; set; } = "CaseReference";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var model = context.ActionArguments.SingleOrDefault(_ => _.Key == "model").Value;
            var caseReference = (string)model.GetType().GetProperty(CaseReferencePropertyName).GetValue(model, null);

            var fosteringService = (IFosteringService)context.HttpContext.RequestServices.GetService(typeof(IFosteringService));

            FosteringCase response;
            try
            {
                response = fosteringService.GetCase(caseReference).Result;
            }
            catch (Exception error)
            {
                throw new Exception($"BlockHomeVisitUpdate: Error getting case with reference { caseReference }", error.InnerException);
            }

            try
            {
                if (DateTime.Compare(DateTime.Now, response.HomeVisitDateTime.GetValueOrDefault().AddHours(-0.5)) >= 0)
                {
                    context.Result = new Http423Result();
                }
            }
            catch (Exception error)
            {
                var logger = (ILogger<BlockHomeVisitUpdate>)context.HttpContext.RequestServices.GetService(typeof(ILogger<BlockHomeVisitUpdate>));
                logger.LogWarning($"BlockHomeVisitUpdate: Error comparing home visit date time to current time, case reference {caseReference}, home visit time {response.HomeVisitDateTime.GetValueOrDefault():d}", error);
            }
        }
    }
}