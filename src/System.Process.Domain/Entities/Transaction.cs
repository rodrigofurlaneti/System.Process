using System.Process.Domain.ValueObjects;
using System.Phoenix.Domain;
using System.Collections.Generic;

namespace System.Process.Domain.Entities
{
    public class Transaction : BaseEntity<string>
    {
        public string Label { get; set; }
        public IList<TransactionCategory> TransactionCategories { get; set; }
    }
}
