using MediatR;

namespace System.Process.Application.Queries.FindReceivers
{
    public class FindReceiversRequest : IRequest<FindReceiversResponse>
    {
        public string CustomerId { get; set; }
        public string InquiryType { get; set; }
        public string Ownership { get; set; }
    }
}
