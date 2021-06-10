namespace System.Process.Domain.Entities
{
    public class TransferItem
    {
        public int? TransferItemId { get; set; }
        public string SystemId { get; set; }
        public string LifeCycleId { get; set; }
        public string ReferenceId { get; set; }
        public decimal Amount { get; set; }
        public string FrontImage { get; set; }
        public string RearImage { get; set; }
    }
}
