using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Metamory.Api.Repositories
{
    public interface IContentRepository
    {
        Task<string> DownloadContentToStreamAsync(string siteId, string contentId, string versionId, Stream memoryStream);

        Task AddContentAsync(string siteId, string contentId, string versionId, Stream contentStream, string contentType, DateTimeOffset now, string previousVersionId, string author, string label);

        Task<IEnumerable<ContentMetadataEntity>> GetVersionsAsync(string siteId, string contentId);
    }
}