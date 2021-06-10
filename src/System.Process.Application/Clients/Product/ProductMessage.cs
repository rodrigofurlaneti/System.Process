using System.Process.Application.DataTransferObjects;

namespace System.Process.Worker.Clients.Product
{
    public class ProductMessage
    {
        public string AccountType { get; set; }
        public string ProductCode { get; set; }
        public int QuantityOfNumberProcess { get; set; }
        public string BranchCode { get; set; }
        public DepositInformationRecordDto DepositInformationRec { get; set; }
        public DepositAccountInformationDto DepositAccountInfo { get; set; }
        public DepositOverdrawInformationDto DepositNonSufficientOverdraftsInfo { get; set; }
        public DepositStatementDto DepositStatementInfo { get; set; }
        public DepositAddDto DepositAdd { get; set; }
    }
}
