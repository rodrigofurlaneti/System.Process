using System.Collections.Generic;

namespace System.Process.Application.Commands.CreditCard
{
    public class CreditCardResponse
    {
        public bool Success { get; set; }
        public string AssetId { get; set; }
        public IList<ErrorMessage> ErrorMessages { get; set; }
    }
}
