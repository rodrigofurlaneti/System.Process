using MediatR;

namespace System.Process.Application.Queries.ConsultCardsByCustomerId
{
    public class ConsultCardsByCustomerIdRequest : IRequest<ConsultCardsByCustomerIdResponse>
    {
        public string CustomerId { get; set; }
        public string CardType { get; set; }

        public ConsultCardsByCustomerIdRequest(string customerId, string type)
        {
            CustomerId = customerId;
            CardType = type ?? "DR";
        }
    }
}
