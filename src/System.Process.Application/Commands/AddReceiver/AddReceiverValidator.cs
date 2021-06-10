using FluentValidation;
using System.Process.Domain.Enums;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace System.Process.Application.Commands.AddReceiver
{
    public class AddReceiverValidator : AbstractValidator<AddReceiverRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public AddReceiverValidator()
        {
            ValidateCustomerId();
            ValidateRoutingNumber();
            ValidateFirstName();
            ValidateLastName();
            ValidateAccountNumber();           
            ValidateAccountType();
            ValidatePhoneOrEmail();
            ValidatePhoneNumber();
            ValidateEmailAdress();
            ValidateBankType();
            ValidateOwnership();
        }

        #endregion

        #region Validation

        private void ValidateCustomerId()
        {
            RuleFor(c => c.CustomerId)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} CustomerId");
        }

        private void ValidateRoutingNumber()
        {
            RuleFor(c => c.RoutingNumber)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} ValidateRoutingNumber");
        }

        private void ValidateFirstName()
        {
            RuleFor(c => c.FirstName)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} FirstName");
        }

        private void ValidateLastName()
        {
            RuleFor(c => c.LastName)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} LastName");
        }

        private void ValidateAccountNumber()
        {
            RuleFor(c => c.AccountNumber)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} AccountNumber");
        }

        private void ValidateAccountType()
        {
            RuleFor(c => c.AccountType)
                .NotNull()
                .NotEmpty()
                .Must(source => Enum.IsDefined(typeof(AccountType), source))
                .WithMessage($"{DefaultValidMessage} AccountType");
        }

        private void ValidatePhoneOrEmail()
        {
            RuleFor(c => c)
                .Must(c => !(string.IsNullOrEmpty(c.PhoneNumber) && string.IsNullOrEmpty(c.EmailAdress)))
                .WithMessage($"{DefaultValidMessage} PhoneNumber or EmailAdress");
        }

        private void ValidatePhoneNumber()
        {
            When(c => !(string.IsNullOrEmpty(c.PhoneNumber)), () =>
            {
                RuleFor(c => c)
                    .Must(c => ValidatePhoneNumber(c.PhoneNumber))
                    .WithMessage($"{DefaultValidMessage} PhoneNumber");
            }); ;
        }

        private bool ValidatePhoneNumber(string phone)
        {
            var regexPhone = new Regex(@"^\+[1-9]\d{1,14}$");

            if (phone == null || !regexPhone.IsMatch(phone))
            {
                return false;
            }

            return true;
        }

        private void ValidateEmailAdress()
        {
            When(c => !(string.IsNullOrEmpty(c.EmailAdress)), () =>
           {
               RuleFor(c => c)
                   .Must(c => IsValidEmail(c.EmailAdress))
                   .WithMessage($"{DefaultValidMessage} EmailAdress");
           }); ;
        }

        private void ValidateBankType()
        {
            RuleFor(c => c.BankType)
                .NotNull()
                .NotEmpty()
                .Must(bt => Enum.IsDefined(typeof(OriginAccount), bt))
                .Must(bt => IsValidBankType(bt))
                .WithMessage($"{DefaultValidMessage} BankType");
        }

        private bool IsValidBankType(string bankType)
        {
            var accountType = Enum.TryParse(bankType, out OriginAccount originAccount);

            if (originAccount.Equals(OriginAccount.A))
            {
                accountType = false;
            }

            return accountType;
        }

        private void ValidateOwnership()
        {
            RuleFor(c => c.Ownership)
                .NotNull()
                .NotEmpty()
                .Must(os => Enum.IsDefined(typeof(Ownership), os))
                .Must(os => IsValidOwnership(os))
                .WithMessage($"{DefaultValidMessage} Ownership");
        }

        private bool IsValidOwnership(string ownership)
        {
            var ownershipType = Enum.TryParse(ownership, out Ownership ownershipOutput);

            if (ownershipOutput.Equals(Ownership.A))
            {
                ownershipType = false;
            }

            return ownershipType;
        }      

        public bool IsValidEmail(string email)
        {
            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None);

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
