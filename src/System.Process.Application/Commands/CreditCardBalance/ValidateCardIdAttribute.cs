using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace System.Process.Application.Commands.CreditCardBalance
{
    public class ValidateCardIdAttribute : ActionFilterAttribute
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region OnActionExecuting override

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionArguments.ContainsKey("cardId"))
            {
                string cardId = filterContext.ActionArguments["cardId"] as string;
                if (string.IsNullOrEmpty(cardId))
                {
                    filterContext.Result = new BadRequestObjectResult($"The Card ID cannot be null or empty. {DefaultValidMessage} CardId ");
                }

                if (!string.IsNullOrEmpty(cardId))
                {
                    foreach (char validate in cardId)
                    {
                        if (!char.IsDigit(validate))
                        {
                            filterContext.Result = new BadRequestObjectResult($"The Card ID must contain only numbers. {DefaultValidMessage} CardId ");
                        }
                    }
                }
            }
        }

        #endregion
    }
}
