using FluentValidation;
using System.Process.Application.Commands.RemoteDepositCapture.Message;
using System.Collections.Generic;

namespace System.Process.Application.Commands.RemoteDepositCapture
{
    public class RemoteDepositCaptureCommandValidator : AbstractValidator<RemoteDepositCaptureRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public RemoteDepositCaptureCommandValidator()
        {
            ValidateSystemId();
            ValidateToAccount();
            ValidateToRountingNumber();
            ValidateCount();
            ValidateTotalAmount();
            ValidateItem();
        }

        #endregion

        #region Validation

        private void ValidateSystemId()
        {
            RuleFor(c => c.SystemId)
              .NotNull()
              .NotEmpty()
              .WithMessage($"The 'SystemId' is required. {DefaultValidMessage} 'SystemId'.");
        }

        private void ValidateToAccount()
        {
            RuleFor(c => c.ToAccount)
              .NotNull()
              .NotEmpty()
              .WithMessage($"The 'ToAccount' is required. {DefaultValidMessage} 'ToAccount'.");
        }

        private void ValidateToRountingNumber()
        {
            RuleFor(c => c.ToRoutingNumber)
              .NotNull()
              .NotEmpty()
              .WithMessage($"The 'ToRountingNumber' is required. {DefaultValidMessage} 'ToRountingNumber'.");
        }

        private void ValidateCount()
        {
            RuleFor(c => c.Count)
              .NotNull()
              .NotEmpty()
              .WithMessage($"The 'Count' is required. {DefaultValidMessage} 'Count'.");
        }

        private void ValidateTotalAmount()
        {
            RuleFor(c => c.TotalAmount)
              .NotNull()
              .NotEmpty()
              .WithMessage($"The 'TotalAmount' is required. {DefaultValidMessage} 'TotalAmount'.");
        }

        private void ValidateItem()
        {
            RuleFor(c => c.Item)
              .NotNull()
              .NotEmpty()
              .Must(item=> ValidateItem(item))
              .WithMessage($"The 'Item' is required. {DefaultValidMessage} 'Item'.");
        }

        private bool ValidateItem(List<RequestItem> items)
        {
            foreach(var item in items)
            {
                if (item.Amount <= 0 || item.BackImage == null || item.FrontImage == null)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
