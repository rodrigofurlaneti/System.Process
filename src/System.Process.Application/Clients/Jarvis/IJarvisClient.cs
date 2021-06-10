using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Clients.Jarvis
{
    public interface IJarvisClient
    {
        Task<DeviceDetails> GetDeviceDetails(string sessionId, CancellationToken cancellationToken);
    }
}