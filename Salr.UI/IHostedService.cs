using System.Threading;
using System.Threading.Tasks;

namespace NNostr.UI;

public interface ISimilarHostedService
{
    Task StartAsync(CancellationToken token);
    Task StopAsync(CancellationToken token);
}