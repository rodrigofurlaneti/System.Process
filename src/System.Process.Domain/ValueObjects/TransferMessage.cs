using System;
using System.Collections.Generic;
using System.Text;

namespace System.Process.Domain.ValueObjects
{
    public class TransferMessage
    {
        public string MemoDescription { get; set; }
        public string TransactionDetail { get; set; }
        public bool HasDetails { get; set; } = false;
        public string TransferType { get; set; }
        public string FromBank { get; set; }
        public string FromAccountOwnerName { get; set; }
        public string AccountOwnerName { get; set; }
        public string ToAccountNumber { get; set; }
        public string ToBank { get; set; }
        public string Receipt { get; set; }
        public string Location { get; set; }
        public string Card { get; set; }
        public string ToAba { get; set; }
        public string TransferDate { get; set; }
    }
}
