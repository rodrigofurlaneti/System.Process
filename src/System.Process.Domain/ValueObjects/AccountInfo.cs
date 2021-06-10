using System.Process.Domain.Enums;

namespace System.Process.Domain.ValueObjects
{
    public class AccountInfo
    {
        public string AssetId { get; set; }
        public string MerchantId { get; set; }
        public string RoutingNumber { get; set; }
        public OriginAccount Origin { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
    }
}
