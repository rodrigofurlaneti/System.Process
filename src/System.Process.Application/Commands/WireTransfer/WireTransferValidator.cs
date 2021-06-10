using FluentValidation;
using System.Process.Application.Utils;
using System.Globalization;
using System.Text.RegularExpressions;

namespace System.Process.Application.Commands.WireTransfer
{
    public class WireTransferValidator : AbstractValidator<WireTransferAddRequest>
    {
        #region Attributes

        private static string DefaultValidMessage = "Please ensure you have entered a valid";

        #endregion

        #region Constructor

        public WireTransferValidator()
        {
            ValidateFromAccountId();
            ValidateFromRoutingNumber();
            ValidateFromAccountType();
            ValidateToAccountId();
            ValidateToRoutingNumber();
            ValidateToAccountType();
            ValidateBankName();
            ValidateAmount();
            ValidateReceiverFirstName();
            ValidateReceiverLastName();
            ValidateReceiverContact();
            ValidateReceiverPhone();
            ValidateReceiverEmail();
            ValidateBankAddress();
            ValidateReceiverAddress();
        }

        #endregion

        #region Validation

        private void ValidateFromAccountId()
        {
            RuleFor(c => c.FromAccountId)
                .NotNull()
                .NotEmpty()
                .Must(accNumber => ValidateIsNumber(accNumber))
                .WithMessage($"{DefaultValidMessage} FromAccountNumber");
        }

        private void ValidateFromRoutingNumber()
        {
            RuleFor(c => c.FromRoutingNumber)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} FromRoutingNumber");
        }

        private void ValidateFromAccountType()
        {
            RuleFor(c => c.FromAccountType)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} FromAccountType");
        }

        private void ValidateToAccountId()
        {
            RuleFor(c => c.ToAccountId)
                .NotNull()
                .NotEmpty()
                .Must(accNumber => ValidateIsNumber(accNumber))
                .WithMessage($"{DefaultValidMessage} ToAccountNumber");
        }

        private void ValidateToRoutingNumber()
        {
            RuleFor(c => c.ToRoutingNumber)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} ToRoutingNumber");
        }

        private void ValidateToAccountType()
        {
            RuleFor(c => c.ToAccountType)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} ToAccountType");
        }

        private void ValidateBankName()
        {
            RuleFor(c => c.BankName)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} BankName");
        }

        private void ValidateAmount()
        {
            RuleFor(c => c.Amount)
                .NotNull()
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage($"{DefaultValidMessage} Amount and greather than 0");
        }

        private void ValidateReceiverFirstName()
        {
            RuleFor(c => c.ReceiverFirstName)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} ReceiverFirstName");
        }

        private void ValidateReceiverLastName()
        {
            RuleFor(c => c.ReceiverLastName)
                .NotNull()
                .NotEmpty()
                .WithMessage($"{DefaultValidMessage} ReceiverLastName");
        }

        private void ValidateReceiverContact()
        {
            RuleFor(c => c)
               .Must(c => !(string.IsNullOrEmpty(c.ReceiverPhone) && string.IsNullOrEmpty(c.ReceiverEmail)))
               .WithMessage($"{DefaultValidMessage} ReceiverPhone or ReceiverEmail");
        }

        private void ValidateReceiverPhone()
        {
            When(c => !(string.IsNullOrEmpty(c.ReceiverPhone)), () =>
            {
                RuleFor(c => c)
                    .Must(c => ValidatePhoneNumber(c.ReceiverPhone))
                    .WithMessage($"{DefaultValidMessage} ReceiverPhone");
            });
        }

        private void ValidateReceiverEmail()
        {
            When(c => !(string.IsNullOrEmpty(c.ReceiverEmail)), () =>
            {
                RuleFor(c => c)
                    .Must(c => IsValidEmail(c.ReceiverEmail))
                    .WithMessage($"{DefaultValidMessage} ReceiverEmail");
            });
        }

        private void ValidateBankAddress()
        {
            RuleFor(c => c.BankAddress)
                .NotNull()
                .WithMessage($"{DefaultValidMessage} BankAddress");

            When(c => c.BankAddress != null, () =>
            {
                RuleFor(c => c.BankAddress.City)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage($"{DefaultValidMessage} City");

                RuleFor(c => c.BankAddress.State)
                   .NotNull()
                   .NotEmpty()
                   .WithMessage($"{DefaultValidMessage} State");

                RuleFor(c => c.BankAddress.Country)
                   .NotNull()
                   .NotEmpty()
                   .WithMessage($"{DefaultValidMessage} Country");

                When(x => x.ReceiverAddress?.Country != null, () =>
                {
                    RuleFor(c => c.ReceiverAddress.Country)
                   .Must(country => ValidateCountry(country))
                   .WithMessage("The country field must have three letters (ISO 3166-1)");
                });

                RuleFor(c => c.BankAddress.ZipCode)
                   .NotNull()
                   .NotEmpty()
                   .WithMessage($"{DefaultValidMessage} ZipCode");

                RuleFor(c => c.BankAddress.Line1)
                  .NotNull()
                  .NotEmpty()
                  .WithMessage($"{DefaultValidMessage} Line1");
            });
        }

        private void ValidateReceiverAddress()
        {
            RuleFor(c => c.ReceiverAddress)
                .NotNull()
                .WithMessage($"{DefaultValidMessage} ReceiverAddress");

            When(c => c.ReceiverAddress != null, () =>
            {
                RuleFor(c => c.ReceiverAddress.City)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage($"{DefaultValidMessage} City");

                RuleFor(c => c.ReceiverAddress.State)
                   .NotNull()
                   .NotEmpty()
                   .WithMessage($"{DefaultValidMessage} State");

                RuleFor(c => c.ReceiverAddress.Country)
                   .NotNull()
                   .NotEmpty()
                   .WithMessage($"{DefaultValidMessage} Country");

                When(x => x.ReceiverAddress?.Country != null, () =>
                {
                    RuleFor(c => c.ReceiverAddress.Country)
                   .Must(country => ValidateCountry(country))
                   .WithMessage("The country field must have three letters (ISO 3166-1)");
                });

                RuleFor(c => c.ReceiverAddress.ZipCode)
                 .NotNull()
                 .NotEmpty()
                 .WithMessage($"{DefaultValidMessage} ZipCode");

                RuleFor(c => c.ReceiverAddress.Line1)
                  .NotNull()
                  .NotEmpty()
                  .WithMessage($"{DefaultValidMessage} Line1");
            });
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

        private bool ValidatePhoneNumber(string phone)
        {
            var regexPhone = new Regex(@"^\+[1-9]\d{1,14}$");

            if (phone == null || !regexPhone.IsMatch(phone))
            {
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None);

                string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();

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

        private bool ValidateCountry(string country)
        {
            var adaptedCountry = CountryInfo.GetCountryInfo(country).ShortThree;
            var regex = new Regex(@"/^A(BW|FG|GO|IA|L[AB]|ND|R[EGM]|SM|T[AFG]|U[ST]|ZE)|B(DI|E[LNS]|FA|G[DR]|H[RS]|IH|L[MRZ]|MU|OL|R[ABN]|TN|VT|WA)|C(A[FN]|CK|H[ELN]|IV|MR|O[DGKLM]|PV|RI|U[BW]|XR|Y[MP]|ZE)|D(EU|JI|MA|NK|OM|ZA)|E(CU|GY|RI|S[HPT]|TH)|F(IN|JI|LK|R[AO]|SM)|G(AB|BR|EO|GY|HA|I[BN]|LP|MB|N[BQ]|R[CDL]|TM|U[FMY])|H(KG|MD|ND|RV|TI|UN)|I(DN|MN|ND|OT|R[LNQ]|S[LR]|TA)|J(AM|EY|OR|PN)|K(AZ|EN|GZ|HM|IR|NA|OR|WT)|L(AO|B[NRY]|CA|IE|KA|SO|TU|UX|VA)|M(A[CFR]|CO|D[AGV]|EX|HL|KD|L[IT]|MR|N[EGP]|OZ|RT|SR|TQ|US|WI|Y[ST])|N(AM|CL|ER|FK|GA|I[CU]|LD|OR|PL|RU|ZL)|OMN|P(A[KN]|CN|ER|HL|LW|NG|OL|R[IKTY]|SE|YF)|QAT|R(EU|OU|US|WA)|S(AU|DN|EN|G[PS]|HN|JM|L[BEV]|MR|OM|PM|RB|SD|TP|UR|V[KN]|W[EZ]|XM|Y[CR])|T(C[AD]|GO|HA|JK|K[LM]|LS|ON|TO|U[NRV]|WN|ZA)|U(GA|KR|MI|RY|SA|ZB)|V(AT|CT|EN|GB|IR|NM|UT)|W(LF|SM)|YEM|Z(AF|MB|WE)$");

            return regex.IsMatch(adaptedCountry);
        }

        #endregion
    }
}
