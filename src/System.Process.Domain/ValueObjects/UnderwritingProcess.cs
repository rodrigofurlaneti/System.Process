using System.Process.Domain.Enums;

namespace System.Process.Domain.ValueObjects
{
    public class UnderwritingProcess
    {
        public UnderwritingProcessStatus Status { get; set; }
        public int Level { get; set; }
        public string ActivationCumulativeCap { get; set; }
        public string ActivationLevel { get; set; }
        public string AverageTransactionValue { get; set; }
        public string BlockCode { get; set; }
        public string FixedReserveOfSalesReserve { get; set; }
        public string FixedReservePledgeReserve { get; set; }
        public string FixedReserveUpFront { get; set; }
        public string FraudCode { get; set; }
        public string FrequencyOfSettlement { get; set; }
        public string FundingDelayDay { get; set; }
        public string MonthlyCap { get; set; }
        public string RiskCategory { get; set; }
        public string RiskRating { get; set; }
        public string RollingReserve { get; set; }
        public string RollingReserveDays { get; set; }
        public decimal? TicketBelowFloorPercent { get; set; }
        public double? DailyDepositAmount { get; set; }
        public double? WeeklyDepositAmount { get; set; }
        public double? KeyEnteredPercent { get; set; }
        public double? ReturnVolumePercent { get; set; }
        public double? ReturnCountPercent { get; set; }
        public int? MaxTKTOnly { get; set; }
        public int? WeeklyBatches { get; set; }
        public decimal? MaxAuthAmmount { get; set; }
        public double? PercentVoiceAuths { get; set; }
        public double? MaxSalesAmount { get; set; }
        public double? MaxReturnAmount { get; set; }
        public double? BatchReturnAmount { get; set; }
        public double? DailyAuthAmount { get; set; }
        public double? DailyAuthPerCard { get; set; }
        public double? DailyAuthDeclinePercent { get; set; }
        public double? DailyAuths { get; set; }
        public double? MTDChargebackPercent { get; set; }
        public double? SameDollarAmountPercent { get; set; }
        public int? CHOccursInBatch { get; set; }
        public double? DailyTransPerCard { get; set; }
        public double? AuthsSameAmount { get; set; }
        public double? DailyRetrievalCount { get; set; }
        public double? DailyRetrievalAmount { get; set; }
        public int? BatchReturnCount { get; set; }
        public string DisbursementIncludeExcludeException { get; set; }
        public bool? InstantFundsEligibility { get; set; }
    }
}
