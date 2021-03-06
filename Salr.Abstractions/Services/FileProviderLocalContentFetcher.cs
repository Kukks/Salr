using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Salr.Abstractions.Contracts;

namespace Salr.Abstractions.Services
{
    public class FileProviderLocalContentFetcher : ILocalContentFetcher
    {
        private readonly IFileProvider _fileProvider;

        public FileProviderLocalContentFetcher(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public Task<Stream> Fetch(string path)
        {
            var fileInfo = _fileProvider.GetFileInfo("_content/Salr.UI/weather.json");

            if (fileInfo != null && fileInfo.Exists)
            {
                return Task.FromResult(fileInfo.CreateReadStream());
            }

            return Task.FromResult<Stream>(null);
        }
    }
}