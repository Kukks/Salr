using System.IO;
using System.Threading.Tasks;

namespace Salr.Abstractions.Contracts
{
    public interface ILocalContentFetcher
    {
        Task<Stream> Fetch(string path);
    }
}