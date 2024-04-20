using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Metamory.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Metamory.Api.Providers.AzureStorage;


public interface IAzureBlobContentRepositoryConfiguration
{
    string AccountName { get; set; }
}


public class AzureBlobContentRepository : IContentRepository
{
    public class Configurator
    {
        public Configurator(IConfigurationSection configuration, IServiceCollection services)
        {
            services.Configure<AzureStorageRepositoryConfiguration>(configuration.GetSection("ProviderConfiguration:AzureStorageRepositoryConfiguration"));
            services.AddTransient<IContentRepository, AzureBlobContentRepository>();
            services.AddTransient<ICanonicalizeService, AzureCanonicalizeService>();
        }
    }


    private static class MetadataKeynames
    {
        public const string PreviousVersion = "PreviousVersion";
        public const string Timestamp = "Timestamp";
        public const string Author = "Author";
        public const string Label = "Label";
    }


    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobContentRepository(IOptions<AzureStorageRepositoryConfiguration> configurationAccessor)
    {
        var configuration = configurationAccessor.Value;

        // var accountName = configuration.AccountName;
        // _blobServiceClient = new BlobServiceClient(
        //                    new Uri($"https://{accountName}.blob.core.windows.net"),
        //                    new DefaultAzureCredential());

        var connectionString = configuration.ConnectionString;
        _blobServiceClient = new BlobServiceClient(connectionString);

    }

    public async Task<string> DownloadContentToStreamAsync(string siteId, string contentId, string versionId, Stream memoryStream)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(siteId);

        var blobname = GetBlobName(contentId, versionId);
        var blobClient = blobContainerClient.GetBlobClient(blobname);

        await blobClient.DownloadToAsync(memoryStream);

        var properties = await blobClient.GetPropertiesAsync();
        return properties.Value.ContentType;
    }

    public async Task AddContentAsync(string siteId, string contentId, string versionId, Stream contentStream, string contentType, DateTimeOffset now, string previousVersionId, string author, string label)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(siteId);

        await blobContainerClient.CreateIfNotExistsAsync();

        var blobname = GetBlobName(contentId, versionId);
        var blobClient = blobContainerClient.GetBlobClient(blobname);

        await blobClient.UploadAsync(contentStream);

        var metadata = new Dictionary<string, string>();
        void setMetadata(string val, string key) { if (!string.IsNullOrWhiteSpace(val)) metadata.Add(key, val); }

        setMetadata(previousVersionId, MetadataKeynames.PreviousVersion);
        setMetadata(now.ToString("o"), MetadataKeynames.Timestamp);
        setMetadata(author, MetadataKeynames.Author);
        setMetadata(label, MetadataKeynames.Label);

        await blobClient.SetMetadataAsync(metadata);

        var properties = await blobClient.GetPropertiesAsync();
        var headers = new BlobHttpHeaders
        {
            ContentType = contentType,

            // Populate remaining headers with 
            // the pre-existing properties
            CacheControl = properties.Value.CacheControl,
            ContentDisposition = properties.Value.ContentDisposition,
            ContentEncoding = properties.Value.ContentEncoding,
            ContentHash = properties.Value.ContentHash
        };
        await blobClient.SetHttpHeadersAsync(headers);

        //TODO: Arjan: Make sure meta is stored
    }


    public async Task<IEnumerable<ContentMetadataEntity>> GetVersionsAsync(string siteId, string contentId)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(siteId);

        if (!await blobContainerClient.ExistsAsync())
        {
            return Enumerable.Empty<ContentMetadataEntity>();
        }

        var segmentSize = 3;    //TODO; how to ensure we get list of ALL relevant blobs...
        var resultSegment = blobContainerClient.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, contentId + "/").AsPages(default, segmentSize);

        static string getMetadataIfExists(IDictionary<string, string> metadata, string key) => metadata.ContainsKey(key) ? metadata[key] : null;

        var versionList = new List<ContentMetadataEntity>();
        await foreach (Page<BlobItem> blobPage in resultSegment)
        {
            foreach (var blobItem in blobPage.Values)
            {
                //TODO:
                Console.WriteLine("Blob name: {0}", blobItem.Name);

                versionList.Add(new ContentMetadataEntity
                {
                    VersionId = blobItem.Name.Split('/')[1],
                    PreviousVersionId = getMetadataIfExists(blobItem.Metadata, MetadataKeynames.PreviousVersion),
                    Timestamp = DateTimeOffset.Parse(blobItem.Metadata[MetadataKeynames.Timestamp]),
                    Author = getMetadataIfExists(blobItem.Metadata, MetadataKeynames.Author),
                    Label = getMetadataIfExists(blobItem.Metadata, MetadataKeynames.Label)
                });
            }
        }

        return versionList;

        //     var options = new BlobRequestOptions();
        //     BlobContinuationToken token = null;
        //     var versionList = new List<VersionCargo>();
        //     do
        //     {
        //         var res = await contentDirectory.ListBlobsSegmentedAsync(false, BlobListingDetails.Metadata, null, token, options, null);
        //         versionList.AddRange(res.Results
        //             .Cast<CloudBlockBlob>()
        //             .Select(x => new VersionCargo
        //             {
        //                 Version = x.Name.Split('/')[1],
        //                 PreviousVersion = getMetadataIfExists(x.Metadata, MetadataKeynames.PreviousVersion),
        //                 Timestamp = DateTimeOffset.Parse(x.Metadata[MetadataKeynames.Timestamp]),
        //                 Author = getMetadataIfExists(x.Metadata, MetadataKeynames.Author),
        //                 Label = getMetadataIfExists(x.Metadata, MetadataKeynames.Label)
        //             }));

        //         token = res.ContinuationToken;
        //     } while (token != null);

        //     return versionList;
    }


    private static string GetBlobName(string contentId, string versionId)
    {
        return string.Format("{0}/{1}", contentId, versionId);
    }

    public async Task<IEnumerable<string>> ListContentAsync(string siteId)
    {
        //TODO:
        throw new NotImplementedException();
    }
}