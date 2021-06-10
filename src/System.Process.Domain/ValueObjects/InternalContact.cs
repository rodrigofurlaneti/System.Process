using System.Collections.Generic;

namespace System.Process.Domain.ValueObjects
{
    public class InternalContact
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NameSuffix { get; set; }
        public IList<Contact> Contacts { get; set; }
    }
}