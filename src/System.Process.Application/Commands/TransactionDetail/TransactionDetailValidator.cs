using FluentValidation;

namespace System.Process.Application.Commands.TransactionDetail
{
    public class TransactionDetailValidator : AbstractValidator<TransactionDetailRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public TransactionDetailValidator()
        {
            ValidatePan();
            ValidatePrimaryKey();
        }

        #endregion

        #region Validation

        private void ValidatePan()
        {
            RuleFor(c => c.Pan)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Pan");
        }

        private void ValidatePrimaryKey()
        {
            RuleFor(c => c.PrimaryKey)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Primary Key");
        }

        #endregion
    }
}
