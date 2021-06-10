using MediatR;
using System.Process.Domain.ValueObjects;

namespace System.Process.Application.Commands.CardReplace
{
    public class CardReplaceRequest : IRequest<CardReplaceResponse>
    {
        public int CardId { get; set; }
        public string Pan { get; set; }
        public Address Address { get; set; }
        public string ReplaceReason { get; set; }
    }
}
