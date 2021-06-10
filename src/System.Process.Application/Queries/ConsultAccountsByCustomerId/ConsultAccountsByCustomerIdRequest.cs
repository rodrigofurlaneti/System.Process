using MediatR;

namespace System.Process.Application.Queries.ConsultProcessByCustomerId
{
    public class ConsultProcessByCustomerIdRequest : IRequest<ConsultProcessByCustomerIdResponse>
    {
        public string ApplicationId { get; set; }

        public ConsultProcessByCustomerIdRequest(string applicationId)
        {
            ApplicationId = applicationId;
        }
    }
}
