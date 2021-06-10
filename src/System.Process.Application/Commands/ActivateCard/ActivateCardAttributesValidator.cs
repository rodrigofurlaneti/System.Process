using FluentValidation;

namespace System.Process.Application.Commands.ActivateCard
{
    public class ActivateCardAttributesValidator : AbstractValidator<ActivateCardRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public ActivateCardAttributesValidator()
        {
            ValidateCardId();
            ValidatePan();
            ValidateExpireDate();
        }

        #endregion

        #region Validation
        private void ValidateCardId()
        {
            RuleFor(c => c.CardId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"The 'cardId' is required. {DefaultValidMessage} 'cardId'.");

            RuleFor(c => c.CardId)
                .GreaterThan(0)
                .WithMessage($"The 'cardId' should be a number greather than 0. {DefaultValidMessage} 'cardId'.");
        }

        private void ValidatePan()
        {
            RuleFor(c => c.Pan)
                .NotNull()
                .NotEmpty()
                .WithMessage($"The 'pan' is required. {DefaultValidMessage} 'pan'.");

            RuleFor(c => c.Pan)
                .Length(4, 4)
                .WithMessage($"The 'pan' should have 4 digits. {DefaultValidMessage} 'pan'.");
        }

        private void ValidateExpireDate()
        {
            RuleFor(c => c.ExpireDate)
                .NotNull()
                .NotEmpty()
                .WithMessage($"The 'expireDate' is required. {DefaultValidMessage} 'expireDate'.");

            RuleFor(c => c.Pan)
                .Length(4, 4)
                .WithMessage($"The 'expireDate' should have 4 digits. {DefaultValidMessage} 'expireDate'.");
        }
        #endregion
    }
}
