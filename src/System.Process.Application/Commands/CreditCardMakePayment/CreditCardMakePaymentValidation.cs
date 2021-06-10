using FluentValidation;

namespace System.Process.Application.Commands.CreditCardMakePayment
{
    public class CreditCardMakePaymentValidation : AbstractValidator<CreditCardMakePaymentRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public CreditCardMakePaymentValidation()
        {
            ValidateCardId();
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

        #endregion
    }
}
