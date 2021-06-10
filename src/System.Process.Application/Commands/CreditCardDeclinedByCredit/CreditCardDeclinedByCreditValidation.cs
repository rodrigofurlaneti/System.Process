using FluentValidation;

namespace System.Process.Application.Commands.CreditCardDeclinedByCredit
{
    public class CreditCardDeclinedByCreditValidation : AbstractValidator<CreditCardDeclinedByCreditRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public CreditCardDeclinedByCreditValidation()
        {
            ValidateAssetId();
        }

        #endregion

        #region Validations

        private void ValidateAssetId()
        {
            RuleFor(c => c.AssetId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} AssetId");
        }
        #endregion
    }
}
