using FluentValidation;

namespace System.Process.Application.Commands.ChangeCardPin
{
    public class ChangeCardPinAttributesValidator : AbstractValidator<ChangeCardPinRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public ChangeCardPinAttributesValidator()
        {
            ValidateCustomerId();
            ValidateCardId();
            ValidateNewPin();
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

        private void ValidateCustomerId()
        {
            RuleFor(c => c.CustomerId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"The 'CustomerId' is required. {DefaultValidMessage} 'CustomerId'.");
        }

        private void ValidateNewPin()
        {
            RuleFor(c => c.NewPin)
                .NotNull()
                .NotEmpty()
                .WithMessage($"The 'NewPin' is required. {DefaultValidMessage} 'NewPin'.");

            RuleFor(c => c.NewPin)
                .Length(4, 4)
                .WithMessage($"The 'NewPin' should have 4 digits. {DefaultValidMessage} 'NewPin'.");
        }
        #endregion
    }
}
