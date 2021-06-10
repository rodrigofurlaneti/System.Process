using System.Phoenix.Domain;

namespace System.Process.Domain.Entities
{
    public class Statement : BaseEntity<string>
    {
        public string Description { get; set; }
        public bool Holder { get; set; }
        public bool Merchant { get; set; }
        public bool Active { get; set; }
        public string Source { get; set; }

    }
}
