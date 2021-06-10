using FluentAssertions;
using System.Process.Application.Commands.CreateAccount;
using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Messages;
using System.Process.Worker.Clients.Product;
using System;
using System.Collections.Generic;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CreateAccount
{
    public class CreateAccountParamsAdapterTests
    {
        [Fact(DisplayName = "Create Account Params Adapter Test")]
        public void CreateAccountParamsAdapterTest()
        {
            var input = new CreateAccountParamsAdapter
            {
                Message = new AccountMessage
                {
                    Process = new List<AccountInfo>
                    {
                        new AccountInfo
                        {
                            Number = "number",
                            Origin = OriginAccount.E,
                            RoutingNumber = "number",
                            Type = "type"
                        }
                    },
                    Documents = new List<Document>
                    {
                        new Document
                        {
                            AccountId = "string",
                            LeadId = "string",
                            Name = "string",
                            SynergyId = "string",
                            Type = "string",
                            URL = "string"
                        }
                    },
                    Principals = new List<Principal>
                    {
                        new Principal
                        {
                            Address = new Address{},
                            Bankruptcy = false,
                            BankruptcyText = "none",
                            Cif = "cif",
                            Contacts = new List<Contact>
                            {
                                new Contact()
                            },
                            DateOfBirth = DateTime.Now,
                            FirstName = "name",
                            LastName = "name",
                            MiddleName = "name",
                            NameSuffix = "sufix",
                            Principalship = 0,
                            TaxId = new TaxId {},
                            Title = "title"
                        }
                    },
                    ApplicationId = "Id",
                    BankAccount = new BankAccount
                    {
                        AccountNumber = "Number",
                        BankCode = "code",
                        RoutingNumber = "number"
                    },
                    BusinessCif = "Cif",
                    BusinessInformation = new BusinessInformation
                    {
                        DbaName = "name",
                        TaxId = new TaxId
                        {
                            Number = "number",
                            Type = "type"
                        },
                        EntityType = "type",
                        FormationCountry = "Country",
                        FormationDate = DateTime.Now,
                        FormationState = "State",
                        IndustryCode = "code",
                        LegalName = "Name",
                        NumberOfEmployees = 1,
                        Transaction = new Transaction
                        {
                            MonthlySales = 0,
                            MonthlyTransactions = 1
                        },
                        Website = "site"
                    },
                    BusinessRepresentative = false,
                    CustomerReturnPolicy = "none",
                    CustomerReturnPolicyText = "none",
                    Iso = new Iso
                    {
                        InternalContact = new InternalContact
                        {
                            FirstName = "name",
                            LastName = "name",
                            MiddleName = "name",
                            NameSuffix = "sufix"
                        },
                        Name = "name"
                    },
                    MerchantId = "id",
                    OnboardingStatus = OnboardingStatus.Success,
                    OpenCheckingAccount = false,
                    Order = new Order
                    {
                        OrderNumber = "number",
                        ShippingAddress = new Address
                        {
                            City = "city",
                            Country = "country",
                            Line1 = "line",
                            Line2 = "line",
                            Line3 = "line",
                            State = "state",
                            Type = "type",
                            ZipCode = "code"
                        }
                    },
                    OriginChannel = OriginChannel.BankingApp,
                    PaperApplication = false,
                    Pricing = new System.Process.Domain.ValueObjects.Pricing
                    {
                        Type = "type"
                    },
                    ProcessStep = ProcessStep.CifCreated,
                    SalesforceId = "id",
                    UnderwritingProcess = new UnderwritingProcess
                    {
                        Level = 0,
                        Status = UnderwritingProcessStatus.Activated
                    }
                },
                Request = new ProductMessage
                {
                    AccountType = "aa",
                    BranchCode = "aa",
                    DepositAccountInfo = new DepositAccountInformationDto
                    {
                        ATMCard = "aa",
                        CheckGuaranty = "aa",
                        CloseOnZeroBalance = "aa",
                        HighVolumeAccountCode = "code",
                        LastPostingAccountCode = "code",
                        LstPostAccountCode = "code"
                    },
                    DepositAdd = new DepositAddDto
                    {
                        DepositAccountInfo = new DepositAccountInformationDto
                        {
                            ATMCard = "aa",
                            CheckGuaranty = "aa",
                            CloseOnZeroBalance = "aa",
                            HighVolumeAccountCode = "code",
                            LastPostingAccountCode = "code",
                            LstPostAccountCode = "code"
                        },
                        DepositInformationRecord = new DepositInformationRecordDto
                        {
                            AccountClassificationCode = "code",
                            BusinessCIF = "cif",
                            OpenDate = DateTime.Now,
                            OverDraftPrivilegeOptionType = "type",
                            OverdraftPrvgOption = "option",
                            ServiceChargeAccountReason = "reason",
                            ServiceChargeAccountReasonCode = "code",
                            ServiceChargeWaived = "waived",
                            ServiceChargeWaivedReasonCode = "code",
                            SignatureVerificationCode = "code",
                            SignatureVerifyCode = "code"
                        },
                        DepositNonSufficientOverdraftsInfo = new DepositOverdrawInformationDto
                        {
                            AllowReDepositCode = "code",
                            ChargeODCode = "code",
                            NumberAllowedRedepositItems = 10,
                            RedepositNoticeCode = "code"
                        },
                        DepositStatementInfo = new DepositStatementDto
                        {
                            ImagePrintCheckOrderCode = "code",
                            IncludeCombinedStatement = "statment",
                            InterestCycle = 1,
                            ItemTruncation = "item",
                            NextStatementDate = "date",
                            PrintChekesOrderCode = "code",
                            ServiceChargeCycle = "cycle",
                            StatementCreditInterest = "interest",
                            StatementCycle = "cycle",
                            StatementCycleResetFrequencyCode = "code",
                            StatementFrequency = 1,
                            StatementFrequencyCode = "code",
                            StatementPrintCode = "code",
                            StatementServiceCharge = "charge"
                        },
                    },
                    DepositInformationRec = new DepositInformationRecordDto
                    {
                        AccountClassificationCode = "code",
                        BusinessCIF = "cif",
                        OpenDate = DateTime.Now,
                        OverDraftPrivilegeOptionType = "type",
                        OverdraftPrvgOption = "Option",
                        ServiceChargeAccountReason = "Reason",
                        ServiceChargeAccountReasonCode = "code",
                        ServiceChargeWaived = "Waived",
                        ServiceChargeWaivedReasonCode = "code",
                        SignatureVerificationCode = "code",
                        SignatureVerifyCode = "code"
                    },
                    DepositNonSufficientOverdraftsInfo = new DepositOverdrawInformationDto
                    {
                        AllowReDepositCode = "code",
                        ChargeODCode = "code",
                        NumberAllowedRedepositItems = 1,
                        RedepositNoticeCode = "code"
                    },
                    DepositStatementInfo = new DepositStatementDto
                    {
                        ImagePrintCheckOrderCode = "code",
                        IncludeCombinedStatement = "Statment",
                        InterestCycle = 1,
                        ItemTruncation = "item",
                        NextStatementDate = "date",
                        PrintChekesOrderCode = "code",
                        ServiceChargeCycle = "cycle",
                        StatementCreditInterest = "interest",
                        StatementCycle = "cycle",
                        StatementCycleResetFrequencyCode = "code",
                        StatementFrequency = 1,
                        StatementFrequencyCode = "code",
                        StatementPrintCode = "code",
                        StatementServiceCharge = "charge"
                    },
                    ProductCode = "code",
                    QuantityOfNumberProcess = 1
                }
            };



            input.Should().NotBeEquivalentTo(new CreateAccountParamsAdapter());
        }
    }
}
