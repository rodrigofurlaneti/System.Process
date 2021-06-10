using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace System.Process.Application.Queries.ConsultProcessByAccountId
{
    public class ValidateAccountIdAttribute : ActionFilterAttribute
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionArguments.ContainsKey("accountId"))
            {
                string accountId = filterContext.ActionArguments["accountId"] as string;
                if (string.IsNullOrEmpty(accountId))
                {
                    filterContext.Result = new BadRequestObjectResult($"The AccountId cannot be null or empty. {DefaultValidMessage} AccountId ");
                }
            }
        }
    }
}
