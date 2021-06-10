namespace System.Process.Application.DataTransferObjects
{
    public class DepositAddDto
    {
        public DepositInformationRecordDto DepositInformationRecord { get; set; }
        public DepositStatementDto DepositStatementInfo { get; set; }
        public DepositOverdrawInformationDto DepositNonSufficientOverdraftsInfo { get; set; }
        public DepositAccountInformationDto DepositAccountInfo { get; set; }
    }
}
