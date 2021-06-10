using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace System.Process.Application.Queries.ConsultCardsByCustomerId
{
    public class ValidateConsultCardsByCustomerIdAttribute : ActionFilterAttribute
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionArguments.ContainsKey("customerId"))
            {
                string customerId = filterContext.ActionArguments["customerId"] as string;
                if (string.IsNullOrEmpty(customerId))
                {
                    filterContext.Result = new BadRequestObjectResult($"The CustomerId cannot be null or empty. {DefaultValidMessage} CustomerId");
                }
            }

            if (filterContext.ActionArguments.ContainsKey("cardType"))
            {
                string cardType = filterContext.ActionArguments["cardType"] as string;
                if (cardType.Length == 2 && String.Compare(cardType, "DR", true) != 0 && String.Compare(cardType, "CR", true) != 0)
                {
                    filterContext.Result = new BadRequestObjectResult($"The cardType is not valid. {DefaultValidMessage} CardType");
                }
            }
        }
    }
}
