namespace System.Process.Application.DataTransferObjects
{
    public sealed class DepositStatementDto
    {
        public string IncludeCombinedStatement { get; set; }
        public string StatementCycle { get; set; }
        public int InterestCycle { get; set; }
        public string ServiceChargeCycle { get; set; }
        public string ItemTruncation { get; set; }
        public string PrintChekesOrderCode { get; set; }
        public string StatementCycleResetFrequencyCode { get; set; }
        public string ImagePrintCheckOrderCode { get; set; }
        public string NextStatementDate { get; set; }
        public int StatementFrequency { get; set; }
        public string StatementFrequencyCode { get; set; }
        public string StatementPrintCode { get; set; }
        public string StatementServiceCharge { get; set; }
        public string StatementCreditInterest { get; set; }
    }
}