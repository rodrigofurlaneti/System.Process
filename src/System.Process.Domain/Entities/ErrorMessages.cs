using System.Phoenix.Domain;

namespace System.Process.Domain.Entities
{
    public class ErrorMessages : BaseEntity<string>
    {
        public string Title { get; set; }
        public string Details { get; set; }
        public string ErrorCode { get; set; }
        public bool IsSelfHealing { get; set; }
    }
}
