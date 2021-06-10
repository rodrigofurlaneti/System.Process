using System.Collections.Generic;

namespace System.Process.Domain.ValueObjects
{
    public class Transaction
    {
        public decimal? MonthlySales { get; set; }
        public int? MonthlyTransactions { get; set; }
        public IList<Activity> Activity { get; set; }
        public IList<Scheme> Schemes { get; set; }
    }
}