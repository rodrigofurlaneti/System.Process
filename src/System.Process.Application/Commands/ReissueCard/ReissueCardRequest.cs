using MediatR;
using System.Process.Domain.ValueObjects;

namespace System.Process.Application.Commands.ReissueCard
{
    public class ReissueCardRequest : IRequest<ReissueCardResponse>
    {
        public int CardId { get; set; }
        public string Pan { get; set; }
        public Address Address { get; set; }
        public string ReplaceReason { get; set; }
    }
}
