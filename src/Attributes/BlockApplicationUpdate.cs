using System;
using System.Linq;
using fostering_service.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using StockportGovUK.NetStandard.Models.Models.Fostering;

namespace fostering_service.Attributes
{
    public class BlockApplicationUpdate : ActionFilterAttribute
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
                throw new Exception($"BlockApplicationUpdate: Error getting case with reference { caseReference }", error.InnerException);
            }

            if (!response.EnableAdditionalInformationSection)
            {
                context.Result = new Http423Result();
            }
        }
    }
}