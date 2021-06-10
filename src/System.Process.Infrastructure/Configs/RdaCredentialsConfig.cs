using System.Process.Domain.ValueObjects;

namespace System.Process.Infrastructure.Configs
{
    public class RdaCredentialsConfig
    {
        public string TypeGetCustomersCriteria { get; set; }
        public string TypeAddAccount { get; set; }
        public string TypeAddCustomer { get; set; }
        public string GetProcessCriteriaReferenceId { get; set; }
        public string TypeAuthenticate { get; set; }
        public string TypeUpdateClose { get; set; }
        public string TypeUpdateDelete { get; set; }
        public Credentials Credentials { get; set; }
    }
}
