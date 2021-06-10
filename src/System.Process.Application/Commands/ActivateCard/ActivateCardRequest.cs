using MediatR;

namespace System.Process.Application.Commands.ActivateCard
{
    public class ActivateCardRequest : IRequest<ActivateCardResponse>
    {
        public int CardId { get; set; }
        public string Pan { get; set; }
        public string ExpireDate { get; set; }
        public string CustomerId { get; set; }
    }
}
