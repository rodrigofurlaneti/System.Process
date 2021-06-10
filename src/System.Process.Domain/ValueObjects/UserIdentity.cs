using System.Collections.Generic;

namespace System.Process.Domain.ValueObjects
{
    public class UserIdentity
    {
        public string SystemId { get; set; }
        public Dictionary<string, string> ComplementaryIds { get; set; }
        public string ApplicationId { get; set; }
    }
}
