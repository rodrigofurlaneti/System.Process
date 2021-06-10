using System.ComponentModel;
namespace System.Process.Domain.Enums
{
    public enum TransactionCodes
    {

        [Description("-1")]
        Default = -1,

        [Description("163")]
        ACHCredit=163,

        [Description("183")]
        ACHDebit=183,

        [Description("114")]
        InternalTransferCredit=114,

        [Description("113")]
        InternalTransferDebit=113,

        [Description("17")]
        RemoteDepositCaptureMobile=17,

        [Description("111")]
        WireTransferDebit=111,

        [Description("116")]
        WireTransferCredit=116,

        [Description("112")]
        WireTransferFee = 112,

        [Description("222")]
        ATMDeposit=222,

        [Description("212")]
        ATMDepositDDA=212,

        [Description("227")]
        ATMWithdrawal=227,

        [Description("188")]
        AccountAnalysisCharge=188,

        [Description("123")]
        NSFItemPaid=123,

        [Description("980")]
        MemoDebit = 980,

        [Description("920")]
        MemoCredit = 920,

        [Description("228")]
        DebitCard = 228,

        [Description("229")]
        POSPreAuthorizedDebitDDA = 229,

        [Description("239")]
        POSPreAuthorizedDebitSavings = 239,

        [Description("981")]
        PreAuthMemoHold = 981
    }
}

