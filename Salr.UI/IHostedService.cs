using System.Threading;
using System.Threading.Tasks;

namespace NNostr.UI;

public interface IHostedService
{
    Task StartAsync(CancellationToken token);
    Task StopAsync(CancellationToken token);
}