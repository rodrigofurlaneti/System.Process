using FluentValidation;
using System.Process.Domain.Constants;

namespace System.Process.Application.Queries.GetAccountHistory
{
    public class GetAccountHistoryValidator : AbstractValidator<GetAccountHistoryRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public GetAccountHistoryValidator()
        {
            ValidateStartDate();
            ValidateAccountNumber();
            ValidateAccountType();
            ValidateTransactionsType();
        }

        #endregion

        #region Validations
        private void ValidateStartDate()
        {
            RuleFor(c => c)
                .Must(stDate => ValidateStartDate(stDate))
                .WithMessage($"{DefaultValidMessage} Period");
        }

        private bool ValidateStartDate(GetAccountHistoryRequest request)
        {
            if (request.StartDate > request.EndDate ||
                (request.EndDate != null && request.StartDate == null) ||
                (request.EndDate == null && request.StartDate != null))
            {
                return false;
            }

            return true;
        }

        private void ValidateAccountNumber()
        {
            RuleFor(c => c.AccountNumber)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Account Number");
        }

        private void ValidateAccountType()
        {
            RuleFor(c => c.AccountType)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} Account Type");
        }

        private void ValidateTransactionsType()
        {
            RuleFor(c => c.TransactionsType)
                .Must(transtype => ValidateTransType(transtype))
                .WithMessage($"{DefaultValidMessage} Transaction Type. Possible values are: Credit, Debit.");
        }

        private bool ValidateTransType(string transtype)
        {
            if (transtype != null)
            {
                if (transtype == Constants.Debit || transtype == Constants.Credit)
                {
                    return true;
                }

                return false;
            }
            return true;
        }

        #endregion
    }
}
