namespace System.Process.Application.DataTransferObjects
{
    public sealed class DepositAccountInformationDto
    {
        public string LstPostAccountCode { get; set; }
        public string CheckGuaranty { get; set; }
        public string ATMCard { get; set; }
        public string CloseOnZeroBalance { get; set; }
        public string HighVolumeAccountCode { get; set; }
        public string LastPostingAccountCode { get; set; }
    }
}