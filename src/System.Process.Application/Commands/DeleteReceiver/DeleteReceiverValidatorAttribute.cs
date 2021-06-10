using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace System.Process.Application.Commands.DeleteReceiver
{
    public class DeleteReceiverValidatorAttribute : ActionFilterAttribute
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionArguments.ContainsKey("receiverId"))
            {
                string receiverId = filterContext.ActionArguments["receiverId"] as string;
                if (string.IsNullOrEmpty(receiverId))
                {
                    filterContext.Result = new BadRequestObjectResult($"The ReceiverId cannot be null or empty. {DefaultValidMessage} ReceiverId ");
                }
            }
        }
    }
}
