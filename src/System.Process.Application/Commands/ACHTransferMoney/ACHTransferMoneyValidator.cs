using FluentValidation;

namespace System.Process.Application.Commands.AchTransferMoney
{
    public class AchTransferMoneyValidator : AbstractValidator<AchTransferMoneyRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public AchTransferMoneyValidator()
        {
            ValidateFromAccountNumber();
            ValidateReducedPrincipal();
        }

        #endregion

        #region Validation

        private void ValidateFromAccountNumber()
        {
            RuleFor(c => c.AccountFrom.AccountId)
                .NotNull()
                .NotEmpty()
                .Must(accNumber => ValidateIsNumber(accNumber))
                .WithMessage($"{DefaultValidMessage} FromAccountNumber");
        }

        private void ValidateReducedPrincipal()
        {
            RuleFor(c => c.ReducedPrincipal)
                .NotNull()
                .NotEmpty()
                .Must(c => c == "Y" || c == "N")
                .WithMessage($"{DefaultValidMessage} ReducedPrincipal valid types are: Y or N");
        }


        #endregion

        #region Private Methods

        private bool ValidateIsNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return false;
            };

            foreach (char validate in number)
            {
                if (!char.IsDigit(validate))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
