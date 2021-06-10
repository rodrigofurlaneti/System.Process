namespace System.Process.Application.DataTransferObjects
{
    public sealed class DepositOverdrawInformationDto
    {
        public string ChargeODCode { get; set; }
        public string AllowReDepositCode { get; set; }
        public string RedepositNoticeCode { get; set; }
        public int NumberAllowedRedepositItems { get; set; }
    }
}