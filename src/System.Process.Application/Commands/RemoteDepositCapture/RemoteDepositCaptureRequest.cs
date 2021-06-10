using System.Collections.Generic;
using MediatR;
using System.Process.Application.Commands.RemoteDepositCapture.Message;

namespace System.Process.Application.Commands.RemoteDepositCapture
{
    public class RemoteDepositCaptureRequest : IRequest<RemoteDepositCaptureResponse>
    {
        public string SystemId { get; set; }
        public string ToAccount { get; set; }
        public string ToRoutingNumber { get; set; }
        public long? GeoLocation { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public List<RequestItem> Item { get; set; }
        public string SessionId { get; set; }
    }
}
