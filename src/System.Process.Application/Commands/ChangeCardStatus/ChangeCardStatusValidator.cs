using FluentValidation;
using System;

namespace System.Process.Application.Commands.ChangeCardStatus
{
    public class ChangeCardStatusValidator : AbstractValidator<ChangeCardStatusRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public ChangeCardStatusValidator()
        {
            ValidateCardId();
            ValidateAction();
        }

        #endregion

        #region Validation

        private void ValidateCardId()
        {
            RuleFor(c => c.CardId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Card Id");
        }

        private void ValidateAction()
        {
            RuleFor(c => c.Action)
                .Must(action => ValidateAction(action))
                .WithMessage($"{DefaultValidMessage} Action. Possible values are: lock, unlock.");
        }

        private bool ValidateAction(string action)
        {
            if (action != null)
            {
                if (String.Equals(action.ToLower(), "lock") || String.Equals(action.ToLower(), "unlock"))
                {
                    return true;
                }

                return false;
            }
            return true;
        }

        #endregion
    }
}
