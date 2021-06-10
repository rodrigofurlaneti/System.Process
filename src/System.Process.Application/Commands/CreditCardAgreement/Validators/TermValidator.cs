using FluentValidation;
using System.Process.Application.DataTransferObjects;

namespace System.Process.Application.Commands.CreditCardAgreement.Validators
{
    public class TermValidator : AbstractValidator<TermDto>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public TermValidator()
        {
            ValidateAcceptanceDate();
            ValidateType();
        }

        #endregion

        #region Validation

        private void ValidateAcceptanceDate()
        {
            RuleFor(c => c.AcceptanceDate)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} AcceptanceDate");
        }

        private void ValidateType()
        {
            RuleFor(c => c.Type)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Type");
        }

        #endregion
    }
}
