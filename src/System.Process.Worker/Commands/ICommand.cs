using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Worker.Commands
{
    public interface ICommand
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
