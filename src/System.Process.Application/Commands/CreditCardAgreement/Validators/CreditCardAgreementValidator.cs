using FluentValidation;

namespace System.Process.Application.Commands.CreditCardAgreement.Validators
{
    public class CreditCardAgreementValidator : AbstractValidator<CreditCardAgreementRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public CreditCardAgreementValidator()
        {
            ValidateAssetId();
            ValidateSystemId();
            ValidateTerms();
            ValidateAddress();
            ValidateCity();
            ValidateCountry();
            ValidateLine1();
            ValidateState();
            ValidateZipCode();
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

        private void ValidateTerms()
        {
            RuleFor(c => c.Terms)
             .NotNull()
             .NotEmpty()
             .WithMessage($"{DefaultValidMessage} Terms");

            When(c => c.Terms != null, () =>
            {
                RuleForEach(c => c.Terms)
                    .SetValidator(new TermValidator());
            });
        }

        #endregion

        #region Address validations

        private void ValidateAddress()
        {
            RuleFor(c => c.Address)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Address");
        }

        private void ValidateCity()
        {
            When(c => c.Address != null, () =>
            {
                RuleFor(c => c.Address.City)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage($"{DefaultValidMessage} City");
            });
        }

        private void ValidateCountry()
        {
            When(c => c.Address != null, () =>
            {
                RuleFor(c => c.Address.Country)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage($"{DefaultValidMessage} Country");
            });
        }

        private void ValidateLine1()
        {
            When(c => c.Address != null, () =>
            {
                RuleFor(c => c.Address.Line1)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage($"{DefaultValidMessage} Line1");
            });
        }

        private void ValidateState()
        {
            When(c => c.Address != null, () =>
            {
                RuleFor(c => c.Address.State)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage($"{DefaultValidMessage} State");
            });
        }

        private void ValidateZipCode()
        {
            When(c => c.Address != null, () =>
            {
                RuleFor(c => c.Address.ZipCode)
                    .NotNull()
                    .NotEmpty()
                    .Must(zipCode => ValidateIsNumber(zipCode))
                    .WithMessage($"{DefaultValidMessage} ZipCode");
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

        #endregion
    }
}
