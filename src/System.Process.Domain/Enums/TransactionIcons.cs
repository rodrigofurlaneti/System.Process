namespace System.Process.Domain.Enums
{
    public enum TransactionIcons
    {
        [Name("Other")]
        Other = 5,
        OtherSecond = 25,

        [Name("Refund/Reward")]
        Refund = 6,
        RefundSecond = 26,

        [Name("Cash")]
        Cash = 7,
        CashSecond = 27,

        [Name("Fee/Charge")]
        FeeCharge = 60,
        FeeChargeSecond = 61,
        FeeChargeThird = 62,
        FeeChargeFourth = 64,
        FeeChargeFifth = 71,
        FeeChargeSixth = 72,
        FeeChargeSeventh = 79,
        FeeChargeEighth = 80,
        FeeChargeNinth = 81,
        FeeChargeTenth = 82,
        FeeChargeEleventh = 86,

        [Name("Not Applicable to System")]
        NotApplicable = 63,

        [Name("Statement")]
        Statement = 65,
        StatementSecond = 75,
        StatementThird = 85,
    }
}
