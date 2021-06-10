using System;
using System.Collections.Generic;
using System.Process.Application.Clients.Jarvis;
using System.Process.Application.Commands.TransferMoney;
using System.Process.Application.Commands.TransferMoney.Request;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Proxy.Feedzai.Base.Config;
using System.Proxy.Feedzai.Base.Messages;
using System.Proxy.Feedzai.TransferInitiation.Messages;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.SearchAddress.Messages;
using System.Proxy.Silverlake.Base.Config;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Proxy.Silverlake.Inquiry.ServiceReference;
using System.Proxy.Silverlake.Transaction.Common;
using System.Proxy.Silverlake.Transaction.Messages;
using System.Proxy.Silverlake.Transaction.Messages.Request;
using System.Proxy.Silverlake.Transaction.Messages.Response;

namespace System.Process.Base.UnitTests
{
    public class FakeData
    {
        public Receiver GetReceiver()
        {
            return new Receiver
            {
                AccountNumber = "101010",
                AccountType = "A",
                BankType = "S",
                CompanyName = "Wayne Corp",
                CustomerId = "1111",
                Email = "batman@waynecorp.com",
                FirstName = "Bruce",
                LastName = "Wayne",
                NickName = "Bat",
                Ownership = "Wayne´s Mansion",
                ReceiverType = "R",
                ReceiverId = 123,
                RoutingNumber = "987645",
                Phone = "+12025550185"                
            };
        }

        public List<Receiver> GetReceivers()
        {
            var receivers = new List<Receiver>();

            receivers.Add(GetReceiver());

            return receivers;
        }        

        public TransferAddValidateResponse GetTransferAddValidateResponse()       
        {
            return new TransferAddValidateResponse
            {
                ResponseHeaderInfo = GetResponseMessageHeaderInfo(),
                ResponseStatus = "Success"
            };
        }

        public ResponseMessageHeaderInfo GetResponseMessageHeaderInfo()
        {
            return new ResponseMessageHeaderInfo
            {
                JxChangeHeaderInfo = GetChangeHeaderInfoMessage(),
                RecordInformationMessage = GetRecnfoMessages()
            };
        }

        public List<RecInfoMessage> GetRecnfoMessages()
        {
            return new List<RecInfoMessage>
            {
                GetRecInfoMessage()
            };
        }

        public RecInfoMessage GetRecInfoMessage()
        {
            return new RecInfoMessage
            {
                ErrorCategory = "ErrorCategory",
                ErrorCode = "ErrorCode",
                ErrorDescription = "ErrorDescription",
                ErrorElement = "ErrorElement",
                ErrorElementValue = "ErrorElementValue",
                ErrorLocation = "ErrorLocation"
            };
        }

        public ChangeHeaderInfoMessage GetChangeHeaderInfoMessage()
        {
            return new ChangeHeaderInfoMessage { 
            AuditUserId = "AuditUserId",
            AuditWorkstationId = "AuditWorkstationId",
            AuthenticationUserId = "AuthenticationUserId",
            BusinessCorrelationId = "BusinessCorrelationId",
            ConsumerName = "",
            ConsumerProduct = "",
            InstEnvironment = "InstEnvironment",
            InstRtId = "InstRtId",
            JXLogTrackingId = "JXLogTrackingId",
            JxVersion = "JxVersion",
            ValidConsumerName = "ValidConsumerName",
            ValidConsumerProduct = "ValidConsumerProduct",
            WorkflowCorrelationId = "WorkflowCorrelationId"
            };
        }

        public BaseResult<TransferInitiationResult> GetBaseResultTransferInitiationResult()
        {
            return new BaseResult<TransferInitiationResult>
            {
                ErrorMessage = "ErrorMessage",
                IsSuccess = true,
                ItemReferenceId = "ItemReferenceId",
                Message = "Message",
                Result = GetTransferInitiationResult()
            };
        }

        public TransferInitiationResult GetTransferInitiationResult()
        {
            return new TransferInitiationResult 
            { 
            ActionCodes = new List<string>() { "C1", "C2", "C3"},
            Alert = false,
            Code = "Code",
            Decision = "Approved",
            Errors = new List<string>() { "E1", "E2", "E3"},
            EventExternalId = "EventExternalId",
            LifecycleId = "LifecycleId",
            Score = 1,
            SecondaryActionCodes = new List<string>() { "C4", "C5"},
            Status = "Status",
            Warnings = new List<string>() { "W1", "W2"}
            };
        }

        public ProcessearchResponse GetProcessearchResponse()
        {
            return new ProcessearchResponse
            {
                ProcessearchRecInfo = GetProcessearchRecInfos(),
                AccountType = "AccountType",
                AvlBalCalcCode = "AvlBalCalcCode",
                CustomerId = "CustomerId",
                PersonName = GetPersonNameInfo(),
                ResponseHeaderInfo = GetResponseHeaderInfo(),
                SrchMsgRsHdr = GetSrchMsgRsHdr_CType(),
                TaxId = "TaxId"
            };
        }

        public SrchMsgRsHdr_CType GetSrchMsgRsHdr_CType()
        {
            return new SrchMsgRsHdr_CType();
            
        }

        public ResponseHeaderInfo GetResponseHeaderInfo()
        {
            return new ResponseHeaderInfo
            {
                IsThereMoreRecords = "IsThereMoreRecords",
                JxChangeHeaderInfo = GetHeaderParams(),
                JXchangeLogTrackingId = "JXchangeLogTrackingId",
                MsgRecInfoArray = GetMsgRecInfos(),
                NextRecordCursor = "NextRecordCursor",
                NumberOfSentRecords = 1,
                TotalRecordsExist = 3
            };
        }

        public List<MsgRecInfo> GetMsgRecInfos()
        {
            return new List<MsgRecInfo>
            { 
                GetMsgRecInfo()
            };
        }

        public MsgRecInfo GetMsgRecInfo()
        {
            return new MsgRecInfo
            {
                ErrCat = "ErrCat",
                ErrCode = "ErrCode",
                ErrDesc = "ErrDesc",
                ErrElem = "ErrElem",
                ErrElemVal = "ErrElemVal",
                ErrLoc = "ErrLoc"
            };
        }

        public HeaderParams GetHeaderParams()
        {
            return new HeaderParams
            {
                AuditUserId = "AuditUserId",
                AuditWorkstationId = "AuditWorkstationId",
                AuthenticationUserId = "AuthenticationUserId",
                BusinessCorrelationId = "BusinessCorrelationId",
                ConsumerName = "ConsumerName",
                ConsumerProduct = "ConsumerProduct",
                ErrorCode = new List<string>() { "E1", "E2" },
                InstitutionEnvironment = "InstitutionEnvironment",
                InstitutionRoutingId = "InstitutionRoutingId",
                JXchangeVersion = "JXchangeVersion",
                ValidConsumerName = "ValidConsumerName",
                ValidConsumerProduct = "ValidConsumerProduct",
                WorkflowCorrelationId = "WorkflowCorrelationId"
            };
        }

        public PersonNameInfo.PersonName GetPersonNameInfo()
        {
            return new PersonNameInfo.PersonName
            {
                CommunName = "CommunName",
                ComName = "Wayne Corp",
                FirstName = "FirstName",
                LastName = "LastName",
                MiddleName = "MiddleName",
                XPersonName = GetXPersonName(),
                x_PersonName = GetXPersonName()
            };
        }

        public PersonNameInfo.XPersonName GetXPersonName()
        {
            return new PersonNameInfo.XPersonName
            {
                AbbreviationName = "AbbName",
                LegalName = "LegalName",
                NameSuffix = "NameSuffix",
                SalName = "SalName",
                TitlePrefix = "Title"
            };
        }

        public List<ProcessearchRecInfo> GetProcessearchRecInfos()
        {
            return new List<ProcessearchRecInfo>
            { 
                GetProcessearchRecInfo()
            };
        }

        public ProcessearchRecInfo GetProcessearchRecInfo()
        {
            return new ProcessearchRecInfo
            {
                AccountId = GetAccountId(),
                AccountRelationshipCode = "AccountRelationshipCode",
                AccountRelationshipDesc = "AccountRelationshipDesc",
                Processtatus = "Processtatus",
                ProcesstatusDesc = "ProcesstatusDesc",
                Amount = 0.50M,
                AvailableBalance = 1M,
                CustomerId = "44444",
                PersonName = GetPersonalName_CType(),
                PersonNameInfo = GetPersonNameInfo(),
                ProductCode = "ProductCode",
                ProductDesc = "ProductDesc",
                TaxId = "Taxid",
                TINCode = "TINCode",
                x_CondNotfInfoRec = Getx_CondNotfInfoRec_CType()
            };
        }

        public x_CondNotfInfoRec_CType Getx_CondNotfInfoRec_CType()
        {
            return new x_CondNotfInfoRec_CType
            {
                Any = null,
                CondNotfArray = new CondNotfInfo_CType[] { GetCondNotfInfo_CType() },
                Custom = new Custom_CType()
            };
        }

        private static CondNotfInfo_CType GetCondNotfInfo_CType()
        {
            return new CondNotfInfo_CType
            {
                Any = null,
                CondNotf = new CondNotf_Type(),
                Ver_1 = new Ver_1_CType()
            };
        }

        public PersonName_CType GetPersonalName_CType()
        {
            return GetPersonName_CType();
        }

        private static PersonName_CType GetPersonName_CType()
        {
            return new PersonName_CType
            {
                Ver_1 = new Ver_1_CType(),
                Any = null,
                ComName = GetComName_Type(),
                FirstName = GetFirstName_Type(),
                LastName = GetLastName_Type(),
                MiddleName = GetMiddleName_Type(),
                x_PersonName = Getx_PersonName_CType()
            };
        }

        private static x_PersonName_CType Getx_PersonName_CType()
        {
            return new x_PersonName_CType
            {
                AbbName = GetAbbName_Type(),
                Any = null,
                LegalName = GetLegalName_Type(),
                NameSuffix = GetNameSuffix_Type(),
                SalName = SalName_Type(),
                TitlePrefix = TitlePrefi_Type(),
                Ver_1 = new Ver_1_CType(),
                Ver_2 = new Ver_2_CType()
            };
        }

        private static TitlePrefix_Type TitlePrefi_Type()
        {
            return new TitlePrefix_Type
            {
                Rstr = "Rstr",
                JHANull = "JHANull",
                Value = "Value"
            };
        }

        private static SalName_Type SalName_Type()
        {
            return new SalName_Type
            {
                Value = "Value",
                JHANull = "JHANull",
                Rstr = "Rstr"
            };
        }

        private static NameSuffix_Type GetNameSuffix_Type()
        {
            return new NameSuffix_Type
            {
                Rstr = "Rstr",
                JHANull = "JHANull",
                Value = "Value"
            };
        }

        private static LegalName_Type GetLegalName_Type()
        {
            return new LegalName_Type
            {
                Value = "",
                JHANull = "",
                Rstr = ""
            };
        }

        private static AbbName_Type GetAbbName_Type()
        {
            return new AbbName_Type
            {
                Value = "AbbName_Type"
            };
        }

        private static MiddleName_Type GetMiddleName_Type()
        {
            return new MiddleName_Type
            {
                Value = "Value",
                SrchType = "SrchType",
                Rstr = "Rstr",
                JHANull = "JHANull"
            };
        }

        private static LastName_Type GetLastName_Type()
        {
            return new LastName_Type
            {
                JHANull = "JHANull",
                Rstr = "Rstr",
                SrchType = "SrchType",
                Value = "Value"
            };
        }

        private static FirstName_Type GetFirstName_Type()
        {
            return new FirstName_Type
            {
                Value = "Value",
                SrchType = "SrchType",
                Rstr = "Rstr",
                JHANull = "JHANull"
            };
        }

        private static ComName_Type GetComName_Type()
        {
            return new ComName_Type
            {
                JHANull = "JHANull",
                Rstr = "Rstr",
                SrchType = "SrchType",
                Value = "Value"
            };
        }

        public Proxy.Silverlake.Inquiry.Common.AccountId GetAccountId()
        {
            return new Proxy.Silverlake.Inquiry.Common.AccountId
            {
                AccountNumber = "44444",
                AccountType = "Type"
            };
        }

        public DeviceDetails getDeviceDetails()
        {
            return new DeviceDetails
            {
                Altitude = "Altitude",
                IpAddress = "IpAddress",
                Latitude = "Latitude",
                Longitude = "Longitude",
                MacAddress = "MacAddress",
                Manufacturer = "Manufacturer",
                Model = "Model",
                OsVersion = "OsVersion",
                Platform = "Plataform",
                TimeZone = "TimeZone"
            };
        }

        public BaseResult<GetTokenResult> GetBaseResultGetTokenResult()
        {
            return new BaseResult<GetTokenResult>
            {
                ErrorMessage = "ErrorMessage",
                IsSuccess = true,
                ItemReferenceId = "123",
                Message = "Message",
                Result = GetGetTokenResult()
            };
        }

        public GetTokenResult GetGetTokenResult()
        {
            return new GetTokenResult
            {
                AccessToken = "12345",
                Id = "123",
                InstanceUrl = "url",
                IssuedAt = "IssuedAt",
                Signature = "Signature",
                TokenType = "TokenType"
            };
        }

        public BaseResult<Proxy.Salesforce.Messages.QueryResult<SearchAddressResponse>> GetBaseResultQueryResultSearchAddressResponse()
        {
            return new BaseResult<Proxy.Salesforce.Messages.QueryResult<SearchAddressResponse>>
            {
                ErrorMessage = "ErrorMessage",
                IsSuccess = true,
                ItemReferenceId = "123",
                Message = "Message",
                Result = GetQueryResultSearchAddressResponse()
            };
        }

        public Proxy.Salesforce.Messages.QueryResult<SearchAddressResponse> GetQueryResultSearchAddressResponse()
        {
            return new Proxy.Salesforce.Messages.QueryResult<SearchAddressResponse>
            {
                Errors = GetProxySalesforceErrorMessages(),
                Records = GetSearchAddressResponses()
            };
        }

        public List<SearchAddressResponse> GetSearchAddressResponses()
        {
            return new List<SearchAddressResponse>
            { GetSearchAddressResponse()
            };
        }

        public SearchAddressResponse GetSearchAddressResponse()
        {
            return new SearchAddressResponse
            {
                Account = GetAccount(),
                AddressLine1 = "AddressLine1",
                City = "City",
                Country = "Country",
                Email = "Email",
                Name = "Name",
                State = "State",
                Zip = "12345"
            };
        }

        public Account GetAccount()
        {
            return new Account
            {
                CorporateAddress1 = "CorporateAddress1",
                CorporateCity = "CorporateCity",
                CorporateCountry = "CorporateCountry",
                CorporateState = "CorporateState",
                CorporateZipCode = "CorporateZipCode",
                LegalName = "LegalName",
                SalesforceId = "123"
            };
        }

        public List<Proxy.Salesforce.Messages.ErrorMessage> GetProxySalesforceErrorMessages()
        {
            return new List<Proxy.Salesforce.Messages.ErrorMessage>
            { 
                GetProxySalesforceErrorMessage()
            };
        }

        public Proxy.Salesforce.Messages.ErrorMessage GetProxySalesforceErrorMessage()
        {
            return new Proxy.Salesforce.Messages.ErrorMessage
            {
                ErrorCode = "ErrorCode",
                Fields = new string[] { "F1", "F2", "F3" },
                Message = "Message"
            };
        }

        public TransferMoneyRequest GetTransferMoneyRequest()
        {
            return new TransferMoneyRequest
            {
                AccountFrom = new AccountFrom
                {
                    FromAccountNumber = "44444",
                    FromAccountType = "A"
                },
                AccountTo = new AccountTo
                { 
                    ToAccountNumber = "101010",
                    ToAccountType = "A"
                },
                Amount = 0.05M,
                CustomerId = "44444",
                Message = "Message",
                ReceiverId = "12345",
                ReducedPrincipal = "ReducedPrincipal",
                SystemId = "SystemId",
                SessionId = "SessionId"
            };
        }

        public ProcessConfig GetProcessConfig()
        {
            return new ProcessConfig
            {
                TransferSourceType = "TransferSourceType",
                OfficerCode = "OfficerCode",
                AccountBalance = "AccountBalance",
                AccountClassificationCode = "AccountClassificationCode",
                AccountCommandBranchCode = "AccountCommandBranchCode",
                AccountCommandProductCode = "AccountCommandProductCode",
                Processtatus = "Processtatus",
                ProcesstatusHostValue = "ProcesstatusHostValue",
                AccountType = "AccountType",
                AccountTypeHostValue = "AccountTypeHostValue",
                AchCreditTransactionCodeCode = "AchCreditTransactionCodeCode",
                AchDebitTransactionCodeCode = "AchDebitTransactionCodeCode",
                AchFeeAmount = 10,
                AchNsfCode = "AchNsfCode",
                AchOneTime = "AchOneTime",
                AchSendPreNoteCode = "AchSendPreNoteCode",
                AchTermCount = 3,
                AchTermUnits = "AchTermUnits",
                AchUseLoanAmountCode = "AchUseLoanAmountCode",
                AchUseLoanDateCode = "AchUseLoanDateCode",
                ActivationFlag = "ActivationFlag",
                ActivationStatus = "ActivationStatus",
                AdapterBranchCode = "AdapterBranchCode",
                AdapterProductCode = "AdapterProductCode",
                AddressFormatHostValue = "AddressFormatHostValue",
                AddressType = "AddressType",
                AddressTypeHostValue = "AddressTypeHostValue",
                AllowReDepositCode = "AllowReDepositCode",
                ATMCard = "ATMCard",
                AutoIssueInd = "AutoIssueInd",
                AvailableBalanceCurrency = "AvailableBalanceCurrency",
                AvsData = "AvsData",
                BalanceInquiryAplication = "BalanceInquiryAplication",
                Bin = "Bin",
                CanAccountDeposit = "CanAccountDeposit",
                CanAccountInquiry = "CanAccountInquiry",
                CanAccountPurchase = "CanAccountPurchase",
                CanAccountWithdraw = "CanAccountWithdraw",
                CanDeposit = "CanDeposit",
                CanInquire = "CanInquire",
                CanPurchase = "CanPurchase",
                CanWithdraw = "CanWithdraw",
                CardActivationStatus = "CardActivationStatus",
                CardAssociationHostValue = "CardAssociationHostValue",
                CardCategoryHostValue = "CardCategoryHostValue",
                CardMediaTypeHostValue = "CardMediaTypeHostValue",
                CardOrderFlagHostValue = "CardOrderFlagHostValue",
                CardOrderStatusFlagHostValue = "CardOrderStatusFlagHostValue",
                CardOrderTypeHostValue = "CardOrderTypeHostValue",
                CardOrdFlagHostValue = "CardOrdFlagHostValue",
                CardOrdStatFlagHostValue = "CardOrdStatFlagHostValue",
                CardOrdTypeHostValue = "CardOrdTypeHostValue",
                CardReissueIndicator = "CardReissueIndicator",
                CardReplInd = "CardReplInd",
                CardStatus = new CardStatus
                {
                    Active = "Y",
                    HostValue = "HostValue"
                },
                CardSubTypeHostValue = "CardSubTypeHostValue",
                CardTypeHostValue = "CardTypeHostValue",
                CashLimit = "CashLimit",
                ChargeODCode = "ChargeODCode",
                CheckGuaranty = "CheckGuaranty",
                CloseOnZeroBalance = "CloseOnZeroBalance",
                CompanyId = "CompanyId",
                CompanyName = "Wayne Corp",
                ContactPrefHostValue = "ContactPrefHostValue",
                CreditCardActivationStatus = "CreditCardActivationStatus",
                CreditLimit = "CreditLimit",
                CurrentBalanceCurrency = "CurrentBalanceCurrency",
                DebitTransactionCodeCode = "DebitTransactionCodeCode",
                EncryptKey = "EncryptKey",
                EnterpriseCustomerId = "EnterpriseCustomerId",
                FeeAmount = "FeeAmount",
                FirstCallInd = "FirstCallInd",
                FlatCardInd = "FlatCardInd",
                FundingIndicator = "FundingIndicator",
                HasCardCompanionMicro = "HasCardCompanionMicro",
                HasCardCompanionMini = "HasCardCompanionMini",
                HasCardCompanionMobile = "HasCardCompanionMobile",
                HighVolumeAccountCode = "HighVolumeAccountCode",
                ImagePrintCheckOrderCode = "ImagePrintCheckOrderCode",
                IncludeCombinedStatement = "IncludeCombinedStatement",
                IsPrimaryContact = "IsPrimaryContact",
                ItemTruncation = "ItemTruncation",
                LanguageIndHostValue = "LanguageIndHostValue",
                LimitGroup = "LimitGroup",
                Locked = "Locked",
                LstPostAccountCode = "LstPostAccountCode",
                MemberNumber = "MemberNumber",
                MemberNumberReplaceCard = "MemberNumberReplaceCard",
                NumberAllowedRedepositItems = "NumberAllowedRedepositItems",
                OarSelectType = "OarSelectType",
                OptOutVauAbuFlag = "OptOutVauAbuFlag",
                PaymentsFrom = "PaymentsFrom",
                PaymentsFromAccount = "PaymentsFromAccount",
                PaymentsTo = "PaymentsTo",
                PaymentsToAccount = "PaymentsToAccount",
                PhotoId = "PhotoId",
                PinOffset = "PinOffset",
                PinOffsetIndicator = "PinOffsetIndicator",
                PrimaryIndicator = "PrimaryIndicator",
                PwrOfAttrnyFlag = "PwrOfAttrnyFlag",
                QuantityOfNumberProcess = "QuantityOfNumberProcess",
                ReceiverBankRoutingNumber = "ReceiverBankRoutingNumber",
                RedepositNoticeCode = "RedepositNoticeCode",
                RoutingNumber = "RoutingNumber",
                RtdxParamsConfig = new RtdxParamsConfig
                {
                    CommercialCompanyAdd = new CommercialCompanyAddConfig
                    {
                        AuthroziationLevel = "AuthroziationLevel",
                        CardTwoType = "CardTwoType",
                        CompanyType = "CompanyType",
                        CorpId = "CoprId",
                        DistributionMethodOverride = "DistributionMethodOverride",
                        EmbossingCardholderName = "EmbossingCardholderName",
                        EmbossingName = "EmbossingName",
                        FiscalYear = "FiscalYear",
                        IndustryType = "IndustryType",
                        MembershipFeeFrequencyOne = "MembershipFeeFrequencyOne",
                        MembershipFeeLevelOne = "MembershipFeeLevelOne",
                        MembershipFeeOptionOne = "MembershipFeeOptionOne",
                        MembershipFeeSuppressOne = "MembershipFeeSuppressOne",
                        PinIndicatorOne = "PinIndicatorOne",
                        ReportGroup = "ReportGroup",
                        RetainCreditBalance = "RetainCreditBalance",
                        SendStatement = "SendStatement",
                        Status = "Status"
                    },
                    NewAccountAdd = new NewAccountAddConfig
                    {
                        CardActivationStatus = "CardActivationStatus",
                        CardAnnualFeeIndicator = "CardAnnualFeeIndicator",
                        CardholderVerificationMethodIndTwo = "CardholderVerificationMethodIndTwo",
                        CardTwoType = "CardTwoType",
                        CheckOrderCode = "CheckOrderCode",
                        CheckStartNumber = "CheckStartNumber",
                        CommercialCardIndicator = "CommercialCardIndicator",
                        Corp = "Corp",
                        CreatePlastics = "CreatePlastics",
                        CreditAssociationTwo = "CreditAssociationTwo",
                        CurrencyCode = "CurrencyCode",
                        EmbossNameIndicator = "EmbossNameIndicator",
                        FinanceCharge = "FinanceCharge",
                        FiscalYearEndMonth = "FiscalYearEndMonth",
                        InstitutionId = "InstitutionId",
                        LoyaltyCode = "LoyaltyCode",
                        LoyaltyIndicator = "LoyaltyIndicator",
                        MilitaryLendingActIndP1C = "MilitaryLendingActIndP1C",
                        NumberCardTwoIssueNameTwo = "NumberCardTwoIssueNameTwo",
                        OverLimitOptionIndicator = "OverLimitOptionIndicator",
                        PrimaryCardDeliveryCode = "PrimaryCardDeliveryCode",
                        ProcessingType = "ProcessingType",
                        SMSTextConsentInd = "SMSTextConsentInd",
                        SourceCode = "SourceCode",
                        StatementGroupCode = "StatementGroupCode",
                        SublevelId = "SublevelId",
                        SublevelNumber = "SublevelNumber"
                    },
                    OrderNewPlastic = new OrderNewPlasticConfig
                    {
                        AddressChangeWarning = "AddressChangeWarning",
                        CardDeliveryCode = "CardDeliveryCode",
                        CreatePlastics = "CreatePlastics",
                        NumberCardTwoIssueNameOne = "NumberCardTwoIssueNameOne",
                        NumberCardTwoIssueNameTwo = "NumberCardTwoIssueNameTwo",
                        NumberCardTwoIssueNameTwoAdditional = "NumberCardTwoIssueNameTwoAdditional",
                        PlasticTypeTwo = "PlasticTypeTwo"
                    }                    
                },
                SafraBankingAccountType = "SafraBankingAccountType",
                SafraDigitalBank = "SafraDigitalBank",
                SenderBankRoutingNumber = "SenderBankRoutingNumber",
                SenderTransactionCurrency = "SenderTransactionCurrency",
                ServiceChargeWaived = "ServiceChargeWaived",
                ServiceCodeHostValue = "ServiceCodeHostValue",
                SignatureVerificationCode = "SignatureVerificationCode",
                StatementFrequency = "StatementFrequency",
                StatementFrequencyCode = "StatementFrequencyCode",
                StatementPrintCode = "StatementPrintCode",
                StatementServiceCharge = "StatementServiceCharge",
                TemplateCreated = "TemplateCreated",
                ThirdPartyPymt = "ThirdPartyPymt",
                TransactionCodeCode = "TransactionCodeCode",
                TransferConfig = new TransferConfig
                {
                    AchDirection = "AchDirection",
                    AchPaymentType = "AchPaymentType",
                    ChannelType = "ChannelType",
                    ConsumerNameACH = "ConsumerNameACH",
                    ConsumerProductACH = "ConsumerProductACH",
                    Frequency = "Frequency",
                    GeographicScope = "GeographicScope",
                    PaymentStatus = "PaymentStatus",
                    RdcDirection = "RdcDirection",
                    RdcPaymentSubType = "RdcPaymentSubType",
                    RdcPaymentType = "RdcPaymentType",
                    ReceiverProcesstatus = "ReceiverProcesstatus",
                    ReceiverBankFeeType = "ReceiverBankFeeType",
                    ReceiverCorrespondentBankFeeType = "ReceiverCorrespondentBankFeeType",
                    ReceiverTransactionFeeType = "ReceiverTransactionFeeType",
                    SenderProcesstatus = "SenderProcesstatus",
                    SenderBankFeeType = "SenderBankFeeType",
                    SenderCorrespondentBankFeeType = "SenderCorrespondentBankFeeType",
                    SenderTransactionFeeType = "SenderTransactionFeeType",
                    WireDirection = "WireDirection",
                    WirePaymentSubType = "WirePaymentSubType",
                    WirePaymentType = "WirePaymentType",
                    XferConsumerName = "RTPIT",
                    XferConsumerProd = "realtime",
                    XferDirection = "XferDirection",
                    XferPaymentSubType = "XferPaymentSubType",
                    XferPaymentType = "XferPaymentType"
                },
                TransferFromAccount = "TransferFromAccount",
                TransfersFrom = "TransfersFrom",
                TransfersTo = "TransfersTo",
                TransferToAccount = "TransferToAccount",
                TransferTypeACH = "TransferTypeACH",
                TravelIndHostValue = "TravelIndHostValue",
                WireAnlysCode = "WireAnlysCode"

            };
        }

        public FeedzaiConfig GetFeedzaiConfig()
        {
            return new FeedzaiConfig
            {
                Token = "FeedzaiToken",
                Url = "url"
            };
        }

        public TransferAddValidateRequest GetTransferAddValidateRequest()
        {
            return new TransferAddValidateRequest
            {
                AccountIdFrom = GetAccountIdFrom(),
                AccountIdTo = GetAccountIdTo(),
                AchTransferReceive = GetAchTransferReceive(),
                AuthenUsrId = "",
                BusinessCorrelationId = "",
                ConsumerName = "",
                FutureTransferReceive = new FutureTransferReceive(),
                JxVersion = "",
                TransferReceive = new TransferReceive(),
                TransferType = "",
                WorkflowCorrelationId = ""
            };
        }

        private static AchTransferReceive GetAchTransferReceive()
        {
            return new AchTransferReceive
            {
                AchProcesstatus = "",
                AchAddendaArray = new List<AchAddendaArray>(),
                AchAltCompanyName = "",
                AchCompanyDiscretionaryData = "",
                AchCompanyEntryDescription = "",
                AchCompanyId = "",
                AchCompanyName = "",
                AchCreditAccountId = "",
                AchCreditAccountType = "",
                AchCreditBranchCode = "",
                AchCreditName = "",
                AchCreditRoutingNumber = 3,
                AchCreditTransactionCodeCode = "",
                AchDayAdvance = 3,
                AchDebitAccountId = "",
                AchDebitAccountType = "",
                AchDebitBranchCode = "",
                AchDebitName = "",
                AchDebitRoutingNumber = 3,
                AchDebitTransactionCodeCode = "",
                AchFeeAmount = 0.10M,
                AchFeeAmountLifetodate = 1.0M,
                AchFeeCreditAccountId = "",
                AchFeeCreditAccountType = "",
                AchFeeCreditBranchCode = "",
                AchFeeCreditRoutingNumber = 2,
                AchFeeCreditTransactionCodeCode = "",
                AchFeeDebitAccountId = "",
                AchFeeDebitAccountType = "",
                AchFeeDebitBranchCode = "",
                AchFeeDebitRoutingNumber = 1,
                AchFeeDebitTransactionCodeCode = "",
                AchIndiviualId = "",
                AchInternationalInformationReceive = new AchInternationalInformationReceive(),
                AchLastMainDate = DateTime.Now.AddDays(-10),
                AchLastTransferDate = DateTime.Now.AddDays(-5),
                AchNextTransferDate = DateTime.Now.AddDays(10),
                AchNextTransferDay = 5,
                AchNsfCode = "",
                AchOneTime = "",
                AchOpenDate = DateTime.Now.AddDays(-30),
                AchSemiDay1 = 1,
                AchSemiDay2 = 2,
                AchSendPreNoteCode = "",
                AchStandardEntryClass = "",
                AchTermCount = 3,
                AchTermUnits = "",
                AchTransferAmount = 3M,
                AchTransferAmountLifetodate = 5M,
                AchTransferExpireDate = DateTime.Now.AddDays(1),
                AchTransferMaturityPaymentsCode = "",
                AchUseLoanAmountCode = "",
                AchUseLoanDateCode = "",
                ReducedPrincipal = "",
                TransferBalanceType = ""
            };
        }

        private static AccountIdTo GetAccountIdTo()
        {
            return new AccountIdTo
            {
                ToAccountNumber = "101010",
                ToAccountType = "A"
            };
        }

        private static AccountIdFrom GetAccountIdFrom()
        {
            return new AccountIdFrom
            {
                FromAccountNumber = "101010",
                FromAccountType = "A"
            };
        }

        public TransferAddResponse GetTransferAddResponse()
        {
            return new TransferAddResponse
            {
                ResponseHeaderInfo = GetResponseMessageHeaderInfo(),
                ResponseStatus = "Success",
                TransferKey = "123456789"
            };
        }
    }    
}
