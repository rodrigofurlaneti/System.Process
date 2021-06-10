using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Deposit.Common;
using System.Proxy.Silverlake.Deposit.Messages.Request;
using System;
using System.Collections.Generic;

namespace System.Process.Application.Commands.CreateAccount
{
    public class CreateAccountAdapter : IAdapter<AccountAddRequest, CreateAccountParamsAdapter>
    {
        public AccountAddRequest Adapt(CreateAccountParamsAdapter accountAdapter)
        {
            return new AccountAddRequest
            {
                //AccountType = request.AccountType,
                ProductCode = accountAdapter?.Request?.ProductCode,
                BranchCode = accountAdapter?.Request?.BranchCode,
                DepositInformationRec = new DepositInfoRecRequest
                {
                    AccountClassificationCode = accountAdapter?.Request?.DepositInformationRec?.AccountClassificationCode,
                    CustomerId = accountAdapter?.Message?.BusinessCif,
                    OverDraftPrivilegeOptionType = accountAdapter?.Request?.DepositInformationRec?.OverDraftPrivilegeOptionType,
                    SignatureVerifyCode = accountAdapter?.Request?.DepositInformationRec?.SignatureVerifyCode
                },
                DepositAccountInfo = new DepositAccountInfoRequest
                {
                    CheckGuaranty = accountAdapter?.Request?.DepositAccountInfo?.CheckGuaranty,
                    ATMCard = accountAdapter?.Request?.DepositAccountInfo?.ATMCard,
                    CloseOnZeroBalance = accountAdapter?.Request?.DepositAccountInfo?.CloseOnZeroBalance,
                    HighVolumeAccountCode = accountAdapter?.Request?.DepositAccountInfo?.HighVolumeAccountCode,
                    LastPostingAccountCode = accountAdapter?.Request?.DepositAccountInfo?.LastPostingAccountCode
                },
                DepositNSFODInfo = new DepositNSFODInfoRequest
                {
                    ChangeOverdraftCode = accountAdapter?.Request?.DepositNonSufficientOverdraftsInfo?.ChargeODCode,
                    AllowReDepositCode = accountAdapter?.Request?.DepositNonSufficientOverdraftsInfo?.AllowReDepositCode,
                    ReDepositNotCode = accountAdapter?.Request?.DepositNonSufficientOverdraftsInfo?.RedepositNoticeCode
                },
                DepositStatementInfo = new DepositStatementInfoRequest
                {
                    IncludeCombinedStatement = accountAdapter?.Request?.DepositStatementInfo?.IncludeCombinedStatement,
                    StatementCycle = accountAdapter?.Request?.DepositStatementInfo?.StatementCycle,
                    InterestCycle = accountAdapter?.Request?.DepositStatementInfo?.InterestCycle ?? 0,
                    ItemTruncation = accountAdapter?.Request?.DepositStatementInfo?.ItemTruncation,
                    StatementCycleResetFrequencyCode = accountAdapter?.Request?.DepositStatementInfo?.StatementCycleResetFrequencyCode,
                },
                DepositAdd = new DepositAdd
                {
                    DepositInformationRecord = new DepositInformationRecord
                    {
                        BranchCode = accountAdapter?.Request?.BranchCode,
                        ProdCode = accountAdapter?.Request?.ProductCode,
                        CustomerId = accountAdapter?.Message?.BusinessCif,
                        AccountClassificationCode = accountAdapter?.Request?.DepositAdd?.DepositInformationRecord?.AccountClassificationCode,
                        ServiceChargeWaived = accountAdapter?.Request?.DepositAdd?.DepositInformationRecord?.ServiceChargeWaived,
                        SignatureVerificationCode = accountAdapter?.Request?.DepositAdd?.DepositInformationRecord?.SignatureVerificationCode,
                        ServiceChargeWaivedReasonCode = accountAdapter?.Request?.DepositAdd?.DepositInformationRecord?.ServiceChargeWaivedReasonCode,
                        OverdraftPrivilegeOptionInfoList = new List<OverdraftPrivilegeOption>()
                    {
                        new OverdraftPrivilegeOption()
                        {
                            Value = accountAdapter?.Request?.DepositAdd?.DepositInformationRecord?.OverdraftPrvgOption
                        }
                    }
                    },
                    DepositStatementInfo = new DepositStatementInfo
                    {
                        IncludeCombinedStatement = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.IncludeCombinedStatement,
                        ImagePrintCheckOrderCode = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.ImagePrintCheckOrderCode,
                        ItemTruncation = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.ItemTruncation,
                        StatementCycle = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.StatementCycle,
                        ServiceChargeCycle = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.ServiceChargeCycle,
                        InterestCycle = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.InterestCycle ?? 0,
                        NextStatementDate = Convert.ToDateTime(accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.NextStatementDate),
                        StatementFrequency = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.StatementFrequency ?? 0,
                        StatementFrequencyCode = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.StatementFrequencyCode,
                        StatementPrintCode = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.StatementPrintCode,
                        StatementServiceCharge = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.StatementServiceCharge,
                        StatementCreditInterest = accountAdapter?.Request?.DepositAdd?.DepositStatementInfo?.StatementCreditInterest
                    },
                    DepositNonSufficientOverdraftsInfo = new DepositNonSufficientOverdraftsInfo
                    {
                        ChargeODCode = accountAdapter?.Request?.DepositAdd?.DepositNonSufficientOverdraftsInfo?.ChargeODCode,
                        AllowRedepositCode = accountAdapter?.Request?.DepositAdd?.DepositNonSufficientOverdraftsInfo?.AllowReDepositCode,
                        NumberAllowedRedepositItems = accountAdapter?.Request?.DepositAdd?.DepositNonSufficientOverdraftsInfo?.NumberAllowedRedepositItems ?? 0,
                        RedepositNoticeCode = accountAdapter?.Request?.DepositAdd?.DepositNonSufficientOverdraftsInfo?.RedepositNoticeCode
                    },
                    DepositAccountInfo = new DepositAccountInfo
                    {
                        LstPostAccountCode = accountAdapter?.Request?.DepositAdd?.DepositAccountInfo?.LstPostAccountCode,
                        CheckGuaranty = accountAdapter?.Request?.DepositAdd?.DepositAccountInfo?.CheckGuaranty,
                        ATMCard = accountAdapter?.Request?.DepositAdd?.DepositAccountInfo?.ATMCard,
                        CloseOnZeroBalance = accountAdapter?.Request?.DepositAdd?.DepositAccountInfo?.CloseOnZeroBalance,
                        HighVolumeAccountCode = accountAdapter?.Request?.DepositAdd?.DepositAccountInfo?.HighVolumeAccountCode
                    }
                }
            };
        }
    }
}
