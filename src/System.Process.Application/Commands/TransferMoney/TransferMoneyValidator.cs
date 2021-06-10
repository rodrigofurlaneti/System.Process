using FluentValidation;

namespace System.Process.Application.Commands.TransferMoney
{
    public class TransferMoneyValidator : AbstractValidator<TransferMoneyRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public TransferMoneyValidator()
        {
            ValidateFromAccountNumber();
            ValidateFromAccountType();
            ValidateToAccountNumber();
            ValidateToAccountType();
            ValidateAmount();
            ValidateReducedPrincipal();
        }

        #endregion

        #region Validation

        private void ValidateFromAccountNumber()
        {
            RuleFor(c => c.AccountFrom.FromAccountNumber)
                .NotNull()
                .NotEmpty()
                .Must(accNumber => ValidateIsNumber(accNumber))
                .WithMessage($"{DefaultValidMessage} FromAccountNumber");
        }

        private void ValidateFromAccountType()
        {
            RuleFor(c => c.AccountFrom.FromAccountType)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} FromAccountType");
        }

        private void ValidateToAccountNumber()
        {
            RuleFor(c => c.AccountTo.ToAccountNumber)
                .NotNull()
                .NotEmpty()
                .Must(accNumber => ValidateIsNumber(accNumber))
                .WithMessage($"{DefaultValidMessage} ToAccountNumber");
        }

        private void ValidateToAccountType()
        {
            RuleFor(c => c.AccountTo.ToAccountType)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} ToAccountType");
        }

        private void ValidateAmount()
        {
            RuleFor(c => c.Amount)
                .NotNull()
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage($"{DefaultValidMessage} Amount and greather than 0");
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
