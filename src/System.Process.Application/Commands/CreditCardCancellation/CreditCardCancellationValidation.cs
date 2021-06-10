using FluentValidation;

namespace System.Process.Application.Commands.CreditCardCancellation
{
    public class CreditCardCancellationValidation : AbstractValidator<CreditCardCancellationRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public CreditCardCancellationValidation()
        {
            ValidateAssetId();
            ValidateSystemId();
        }

        #endregion

        #region Validations

        private void ValidateAssetId()
        {
            RuleFor(c => c.AssetId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Asset ID");
        }

        private void ValidateSystemId()
        {
            RuleFor(c => c.SystemId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} SystemId");
        }

        #endregion
    }
}
