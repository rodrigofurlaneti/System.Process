using System;

namespace System.Process.Domain.ValueObjects
{
    public class Shareholder
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Cif { get; set; }
        public string SSN { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public float? OwnershipPercentage { get; set; }
        public string Title { get; set; }
        public string InternetAddress { get; set; }
        public Address Address { get; set; }
    }
}
