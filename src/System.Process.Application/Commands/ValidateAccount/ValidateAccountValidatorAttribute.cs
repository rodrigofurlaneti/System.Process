using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Process.Domain.Enums;
using System;

namespace System.Process.Application.Commands.ValidateAccount
{
    public class ValidateAccountValidatorAttribute : ActionFilterAttribute
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionArguments.ContainsKey("accountId"))
            {
                var accountId = filterContext.ActionArguments["accountId"] as string;
                if (string.IsNullOrEmpty(accountId))
                {
                    filterContext.Result = new BadRequestObjectResult($"The AccountId cannot be null or empty. {DefaultValidMessage} AccountId ");
                }
            }

            if (filterContext.ActionArguments.ContainsKey("accountType"))
            {
                var accountType = filterContext.ActionArguments["accountType"] as string;
                if (string.IsNullOrEmpty(accountType) || !(Enum.IsDefined(typeof(AccountType), accountType)))
                {
                    filterContext.Result = new BadRequestObjectResult($"The AccountType cannot be null or empty. {DefaultValidMessage} AccountType ");
                }
            }
        }
    }
}
