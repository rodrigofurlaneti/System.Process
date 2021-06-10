using MediatR;

namespace System.Process.Application.Commands.ResumeTransfer
{
    public class ResumeTransferRequest : IRequest<ResumeTransferResponse>
    {
        public string LifeCycleId { get; set; }
        public string Decision { get; set; }
        public string Reason { get; set; }
        public string SystemId { get; set; }
    }
}
