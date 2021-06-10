using FluentValidation;
using System;
using System.Linq;

namespace System.Process.Application.Commands.CardReplace
{
    public class CardReplaceValidator : AbstractValidator<CardReplaceRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";
        private static string[] ValidReasons = { "damaged", "lost", "stolen", "fraud" };

        #endregion

        #region Constructor

        public CardReplaceValidator()
        {
            ValidateCardId();
            ValidatePan();
            ValidateAddress();
            ValidateReason();
        }

        #endregion

        #region Validation

        private void ValidateCardId()
        {
            RuleFor(c => c.CardId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} CardId");
        }

        private void ValidatePan()
        {
            RuleFor(c => c.Pan)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Pan");
        }

        private void ValidateAddress()
        {
            RuleFor(c => c.Address).NotNull().WithMessage($"{DefaultValidMessage} Address");

            When(c => (c.Address != null), () =>
            {
                RuleFor(c => c.Address.Type).NotNull().WithMessage($"{DefaultValidMessage} Address Type");
            });
        }

        private void ValidateReason()
        {
            RuleFor(c => c.ReplaceReason)
                .Must(reason => reason != null ? ValidReasons.Contains(reason.ToLower()) : false)
                .WithMessage($"{DefaultValidMessage} Replace Reason");
        }

        #endregion
    }
}
