using System;

namespace System.Process.Domain.ValueObjects
{
    public class DataBusiness
    {
        public string BusinessId { get; set; }
        public string Number { get; set; }
        public string Issuer { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Document { get; set; }
        public string VerifyResult { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}