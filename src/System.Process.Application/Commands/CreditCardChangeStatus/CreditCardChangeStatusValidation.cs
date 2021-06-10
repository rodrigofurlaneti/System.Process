using FluentValidation;

namespace System.Process.Application.Commands.CreditCardChangeStatus
{
    public class CreditCardChangeStatusValidation : AbstractValidator<CreditCardChangeStatusRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public CreditCardChangeStatusValidation()
        {
            ValidateCardId();
            ValidateAction();
        }

        #endregion

        #region Validations

        private void ValidateCardId()
        {
            RuleFor(c => c.CardId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Card ID");
        }

        private void ValidateAction()
        {
            RuleFor(c => c.Action)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Action");
        }

        #endregion
    }
}
