using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Process.Domain.Enums;
using System;

namespace System.Process.Application.Queries.FindReceivers
{
    public class FindReceiversValidatorAttribute : ActionFilterAttribute
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

            if (filterContext.ActionArguments.ContainsKey("inquiryType"))
            {
                string inquiryType = filterContext.ActionArguments["inquiryType"] as string;
                if (string.IsNullOrEmpty(inquiryType) || !(Enum.IsDefined(typeof(OriginAccount), inquiryType)))
                {
                    filterContext.Result = new BadRequestObjectResult(
                        $"The InquiryType cannot be null or empty. {DefaultValidMessage} InquiryType. Valid types are: A, S, E.");
                }
            }

            if (filterContext.ActionArguments.ContainsKey("ownership"))
            {
                string ownership = filterContext.ActionArguments["ownership"] as string;
                if (string.IsNullOrEmpty(ownership) || !(Enum.IsDefined(typeof(Ownership), ownership)))
                {
                    filterContext.Result = new BadRequestObjectResult(
                        $"The Ownership cannot be null or empty. {DefaultValidMessage} Ownership. Valid types are: A, S, O.");
                }
            }
        }
    }
}
