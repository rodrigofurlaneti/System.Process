using FluentValidation;

namespace System.Process.Application.Commands.CreditCard
{
    public class CreditCardValidator : AbstractValidator<CreditCardRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public CreditCardValidator()
        {
            ValidateSystemId();
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

        #endregion
    }
}
