using System;
using System.Linq;
using FluentValidation;

namespace System.Process.Application.Commands.CreditCardReplace.Validators
{
    public class CreditCardReplaceValidator : AbstractValidator<CreditCardReplaceRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";
        private static string[] ValidReasons = { "damaged" };
        private static string[] ValidAddressType = { "home", "business" };
        #endregion

        #region Constructor

        public CreditCardReplaceValidator()
        {
            ValidateAddressType();
            ValidateReason();
        }

        #endregion

        #region Validations


        private void ValidateReason()
        {
            RuleFor(c => c.Reason)
                .Must(reason => reason != null ? ValidReasons.Contains(reason.ToLower()) : false)
                .WithMessage($"{DefaultValidMessage} Replace Reason");
        }

        private void ValidateAddressType()
        {
            RuleFor(c => c.AddressType)
                .NotNull()
                .NotEmpty()
                .Must(type => type != null ? ValidAddressType.Contains(type.ToLower()) : false)
                .WithMessage($"{DefaultValidMessage} Address Type");
        }

        #endregion

    }
}
