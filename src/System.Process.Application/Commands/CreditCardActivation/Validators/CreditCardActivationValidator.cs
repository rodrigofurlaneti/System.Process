using System.Text.RegularExpressions;
using FluentValidation;

namespace System.Process.Application.Commands.CreditCardActivation.Validators
{
    public class CreditCardActivationValidator : AbstractValidator<CreditCardActivationRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public CreditCardActivationValidator()
        {
            ValidateCardId();
            ValidateLastFour();
            ValidateExpireDate();
        }

        #endregion

        #region Validation

        private void ValidateCardId()
        {
            RuleFor(c => c.CardId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Card ID");
        }

        private void ValidateLastFour()
        {
            RuleFor(c => c.LastFour)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} PAN");

            When(c => (c.LastFour != null), () =>
            {
                var quantityDigits = 4;

                RuleFor(c => c.LastFour)
                .Must(c => c.Length.Equals(quantityDigits))
                .WithMessage($"PAN must have 4 digits")
                .Must(taxIdNumber => ValidateIsNumber(taxIdNumber))
                .WithMessage($"PAN must be number");
            });
        }

        private void ValidateExpireDate()
        {
            RuleFor(c => c.ExpireDate)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} expiration date");

            When(c => (c.LastFour != null), () =>
            {
                RuleFor(c => c.ExpireDate)
                    .Must(x => ValidateFormatDate(x))
                    .WithMessage("Please ensure a valid format of the expiration date");
            });
        }

        #endregion

        #region Methods

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

        private bool ValidateFormatDate(string date)
        {
            var regex = new Regex(@"^(0[1-9]|10|11|12)/[0-9]{2}$");

            return regex.IsMatch(date.ToString());
        }

        #endregion
    }
}
