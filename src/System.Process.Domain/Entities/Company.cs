using System;
using MongoDB.Bson;
using System.Phoenix.Domain;

namespace System.Process.Domain.Entities
{
    public class Company : BaseEntity<ObjectId>
    {
        public string SystemId { get; set; }
        public DateTime CreationDate { get; set; }
        public string CompanyId { get; set; }
        public string CifId { get; set; }
    }
}
