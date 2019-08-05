using Microsoft.AspNetCore.Mvc;

namespace fostering_service.Attributes
{
    public class Http423Result : ActionResult
    {
        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = 423;
        }
    }
}
