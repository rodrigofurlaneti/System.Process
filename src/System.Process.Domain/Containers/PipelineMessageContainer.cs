using System.Threading;

namespace System.Process.Domain.Containers
{
    public class PipelineMessageContainer<T>
    {
        public CancellationToken CancellationToken { get; set; }
        public T Message { get; set; }

        public PipelineMessageContainer() { }
        public PipelineMessageContainer(T message, CancellationToken cancellationToken)
        {
            Message = message;
            CancellationToken = cancellationToken;
        }
    }
}
