using System;
using System.Collections.Generic;
using System.Process.Domain.Constants;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.RegisterCard.Messages;
using System.Proxy.Fis.RegisterCard.Messages.Params;
using System.Proxy.Fis.ValueObjects;

namespace System.Process.Application.Commands.CreateAccount
{
    public class CreateCardAdapter : IAdapter<RegisterCardParams, CreateCardParamsAdapter>
    {
        private ProcessConfig ProcessConfig { get; set; }
        public CreateCardAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        public RegisterCardParams Adapt(CreateCardParamsAdapter input)
        {
            return new RegisterCardParams
            {
                Bin = ProcessConfig.Bin,
                Card = new Proxy.Fis.RegisterCard.Messages.Params.Card
                {
                    LimitGroup = ProcessConfig.LimitGroup,
                    CanWithdraw = ProcessConfig.CanWithdraw,
                    CanDeposit = ProcessConfig.CanDeposit,
                    CanInquire = ProcessConfig.CanInquire,
                    PaymentsTo = ProcessConfig.PaymentsTo,
                    PaymentsFrom = ProcessConfig.PaymentsFrom,
                    TransfersTo = ProcessConfig.TransfersTo,
                    TransfersFrom = ProcessConfig.TransfersFrom,
                    CanPurchase = ProcessConfig.CanPurchase,
                    ThirdPartyPymt = ProcessConfig.ThirdPartyPymt,
                    CardStatus = new BaseStatus
                    {
                        Status = Constants.PendingActivation,
                        SubStatus = string.Empty,
                        HostValue = ProcessConfig.CardStatus.HostValue
                    },
                    ActivationStatus = ProcessConfig.ActivationStatus,
                    CardMediaType = new BaseDescription
                    {
                        HostValue = ProcessConfig.CardMediaTypeHostValue, //IP or IC
                    },
                    CardAssociation = new BaseDescription
                    {
                        HostValue = ProcessConfig.CardAssociationHostValue,
                    },
                    CardType = new BaseDescription
                    {
                        HostValue = ProcessConfig.CardTypeHostValue,
                    },
                    CardSubType = new BaseDescription
                    {
                        HostValue = ProcessConfig.CardSubTypeHostValue,
                    },
                    VipInd = string.Empty,
                    CardCategory = new BaseDescription
                    {
                        HostValue = ProcessConfig.CardCategoryHostValue,
                    },
                    HasCardCompanionMini = ProcessConfig.HasCardCompanionMini,
                    HasCardCompanionMicro = ProcessConfig.HasCardCompanionMicro,
                    HasCardCompanionMobile = ProcessConfig.HasCardCompanionMobile,
                    PinOffsetIndicator = ProcessConfig.PinOffsetIndicator,
                    PinOffset = ProcessConfig.PinOffset,
                    ExpirationDate = DateTime.Now.AddDays(10).AddYears(3).ToString("yyMM"),
                    GreetingName = input.Message.Principals[0].FirstName + " " + input.Message.Principals[0].LastName,
                    MobileEnrollment = string.Empty,
                    AvsData = ProcessConfig.AvsData,
                    ActivationMethod = new BaseDescription
                    {
                        HostValue = string.Empty,
                    },
                    OptOutVauAbuFlag = ProcessConfig.OptOutVauAbuFlag,
                    ServiceCode = new BaseDescription
                    {
                        HostValue = ProcessConfig.ServiceCodeHostValue,
                    },
                    AutoIssueInd = ProcessConfig.AutoIssueInd,
                    EmbossedBusiness = input.Message.BusinessInformation.LegalName,
                    ContactPref = new BaseDescription
                    {
                        HostValue = ProcessConfig.ContactPrefHostValue,
                    },
                    CreditLimit = ProcessConfig.CreditLimit,
                    CashLimit = ProcessConfig.CashLimit,
                    PwrOfAttrnyFlag = ProcessConfig.PwrOfAttrnyFlag,
                    TravelInd = new BaseDescription
                    {
                        HostValue = ProcessConfig.TravelIndHostValue,
                    },
                    Addresses = new List<Proxy.Fis.ValueObjects.Address> {
                        new Proxy.Fis.ValueObjects.Address
                        {
                            AddressType = new BaseDescription
                            {
                                HostValue = ProcessConfig.AddressTypeHostValue,
                            },
                            AddressFormat = new BaseDescription
                            {
                                HostValue = ProcessConfig.AddressFormatHostValue,
                            },
                            AddressLine1 = input.Message.BusinessInformation.Addresses[0].Line1,
                            City = input.Message.BusinessInformation.Addresses[0].City,
                            StateProvince = input.Message.BusinessInformation.Addresses[0].State,
                            PostalCode = input.Message.BusinessInformation.Addresses[0].ZipCode,
                            CountryCode = new BaseDescription
                            {
                                HostDescription = input.Message.BusinessInformation.Addresses[0].Country,
                                HostValue = "840"
                            },
                            AddressLine2 = input.Message.BusinessInformation.Addresses[0].Line2,
                            AddressLine3 = input.Message.BusinessInformation.Addresses[0].Line3,
                            AddressLine4 = string.Empty,
                        }
                    },
                    Members = new List<Member> {
                        new Member {
                            MemberNum = ProcessConfig.MemberNumber,
                            IsPrimaryContact = ProcessConfig.IsPrimaryContact,
                            FirstName = input.Message.Principals[0].FirstName,
                            LastName = input.Message.Principals[0].LastName,
                            MiddleInitial = input.Message.Principals[0].MiddleName,
                            Prefix = string.Empty, // Prefix
                            Suffix = string.Empty, //Suffix
                            EmbossedName = input.Message.Principals[0].FirstName + " " + input.Message.Principals[0].LastName,
                            CardOrdFlag = new BaseDescription
                            {
                                HostValue = ProcessConfig.CardOrdFlagHostValue,
                            },
                            CardOrdStatFlag = new BaseDescription
                            {
                                HostValue = ProcessConfig.CardOrdStatFlagHostValue,
                            },
                            CardOrdType = new BaseDescription
                            {
                                HostValue = ProcessConfig.CardOrdTypeHostValue,
                            },
                            CardReplInd = ProcessConfig.CardReplInd,
                            FlatCardInd = ProcessConfig.FlatCardInd,
                            DateOfBirth = input.Message.Principals[0].DateOfBirth?.ToString("yyyy-MM-dd"),
                            SocialSecurityNumber = input.Message.Principals[0].TaxId.Number,
                            LanguageInd = new BaseDescription {
                                HostValue = ProcessConfig.LanguageIndHostValue,
                            },
                            EmailAddress1 = input.Message.Principals[0].Contacts[1].Value,
                            Phones = new List<Phone>
                            {
                                new Phone
                                {
                                    PhoneNum = FormatPhoneNum(input.Message.Principals[0].Contacts[0].Value),
                                    PhoneType = new Description
                                    {
                                        HostValue = input.Message.Principals[0].Contacts[0].Type
                                    },
                                    FirstCallInd = ProcessConfig.FirstCallInd,
                                    SecondCallInd = string.Empty
                                }
                            }
                        }
                    },
                    Process = new List<Account>
                    {
                        new Account
                        {
                            AccountNumber = input.Message.Process[0].Number,
                            AccountType = new BaseDescription {
                                HostValue = ProcessConfig.AccountTypeHostValue,
                            },
                            PrimaryIndicator = ProcessConfig.PrimaryIndicator,
                            FundingIndicator = ProcessConfig.FundingIndicator,
                            Processtatus = new BaseStatus
                            {
                                Status = ProcessConfig.Processtatus,
                                SubStatus = string.Empty,
                                HostValue = ProcessConfig.ProcesstatusHostValue
                            },
                            OarSelectType = new BaseDescription
                            {
                                HostValue = ProcessConfig.OarSelectType
                            },
                            CanAccountWithdraw = ProcessConfig.CanAccountWithdraw,
                            CanAccountDeposit = ProcessConfig.CanAccountDeposit,
                            CanAccountInquiry = ProcessConfig.CanAccountInquiry,
                            PaymentsToAccount = ProcessConfig.PaymentsToAccount,
                            PaymentsFromAccount = ProcessConfig.PaymentsFromAccount,
                            TransferToAccount = ProcessConfig.TransferToAccount,
                            TransferFromAccount = ProcessConfig.TransferFromAccount,
                            CanAccountPurchase = ProcessConfig.CanAccountPurchase,
                            ThirdPartyPayments = ProcessConfig.ThirdPartyPymt,
                            AvailableBalance = string.Empty,
                            CreditLineAmount = string.Empty,
                            OverdraftAmount = string.Empty,
                            OverdraftAccountNumber = string.Empty,
                            OverdraftAccountType = new BaseDescription {
                                HostValue = string.Empty
                            }
                        }
                    }
                },
                EnterpriseCustomerId = ProcessConfig.EnterpriseCustomerId,
                IncludeAccountDetails = true,
                Pan = new Pan
                {
                    Alias = string.Empty,
                    CipherText = string.Empty,
                    PlainText = string.Empty
                },
                Plastic = string.Empty
            };
        }

        private string FormatPhoneNum(string phoneNum)
        {
            var phoneFormated = phoneNum.Contains("+1") ? phoneNum.Replace("+1", "") : phoneNum.Replace("+", "");
            return phoneFormated;
        }
    }
}
