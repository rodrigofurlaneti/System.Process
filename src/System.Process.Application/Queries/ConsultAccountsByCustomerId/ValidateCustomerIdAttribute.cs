using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace System.Process.Application.Queries.ConsultProcessByCustomerId
{
    public class ValidateCustomerIdAttribute : ActionFilterAttribute
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region OnActionExecuting override
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionArguments.ContainsKey("customerId"))
            {
                string customerId = filterContext.ActionArguments["customerId"] as string;
                if (string.IsNullOrEmpty(customerId))
                {
                    filterContext.Result = new BadRequestObjectResult($"The CustomerId cannot be null or empty. {DefaultValidMessage} CustomerId ");
                }
            }
        }

        #endregion
    }
}
