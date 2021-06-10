using System.Process.Domain.ValueObjects;
using System;

namespace System.Process.Application.DataTransferObjects
{
    public class TransactionHistoryDto
    {
        public string Type { get; set; }
        public decimal? Amount { get; set; }
        public string AmountCurrency { get; set; }
        public DateTime? PostedDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string MemoMessage { get; set; }
        public TransferMessage TransferMessage { get; set; }
    }
}
