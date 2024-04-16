using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Metamory.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Metamory.Api.Providers.AzureStorage;


public interface IAzureTableStatusRepositoryConfiguration
{
    string AccountName { get; set; }
}


public class AzureTableStatusRepository : IStatusRepository
{
    public class Configurator
    {
        public Configurator(IConfigurationSection configuration, IServiceCollection services)
        {
            services.Configure<AzureStorageRepositoryConfiguration>(configuration.GetSection("ProviderConfiguration:AzureStorageRepositoryConfiguration"));
            services.AddTransient<IStatusRepository, AzureTableStatusRepository>();
            services.AddTransient<ICanonicalizeService, AzureCanonicalizeService>();
        }
    }



    private string _connectionString;
    // private readonly TableClient _tableClient;
    // private readonly TableServiceClient _tableServiceClient;


    public AzureTableStatusRepository(IOptions<AzureStorageRepositoryConfiguration> configurationAccessor)
    {
        var configuration = configurationAccessor.Value;

        _connectionString = configuration.ConnectionString;
        // _tableServiceClient = new TableServiceClient(connectionString);
        // _tableClient = new TableClient(connectionString, "");
    }


    public async Task AddStatusEntryAsync(string siteId, IContentStatusEntity statusEntry)
    {
        var tableServiceClient = new TableServiceClient(_connectionString);
        tableServiceClient.CreateTableIfNotExists(siteId);
        // var response = await _tableServiceClient.CreateTableIfNotExistsAsync(siteId);
        // var tableClient = response.Value;

        var tableClient = new TableClient(_connectionString, siteId);

        var tableEntity = statusEntry as AzureTableContentStatusEntity ?? new AzureTableContentStatusEntity()
        {
            Timestamp = statusEntry.Timestamp,
            ContentId = statusEntry.ContentId,
            VersionId = statusEntry.VersionId,
            StartTime = statusEntry.StartTime,
            Status = statusEntry.Status,
            Responsible = statusEntry.Responsible,
        };

        // await tableClient.AddEntityAsync(tableEntity);
        tableClient.AddEntity(tableEntity);
    }

    public async Task<IEnumerable<IContentStatusEntity>> GetStatusEntriesAsync(string siteId, string contentId)
    {
        var tableServiceClient = new TableServiceClient(_connectionString);

        var queryTableResults = tableServiceClient.Query(filter: $"TableName eq '{siteId}'");
        if (!queryTableResults.Any()) return Enumerable.Empty<IContentStatusEntity>();

        var tableClient = tableServiceClient.GetTableClient(siteId);
        var statusEntries = tableClient.Query<AzureTableContentStatusEntity>(filter: $"PartitionKey eq '{contentId}'");

        return statusEntries;
    }

    public IContentStatusEntity CreateContentStatusEntity(string contentId, DateTimeOffset timestamp, DateTimeOffset startTime, string versionId, string status, string responsible)
    {
        return new AzureTableContentStatusEntity()
        {
            ContentId = contentId,
            Timestamp = timestamp,
            StartTime = startTime,
            VersionId = versionId,
            Status = status,
            Responsible = responsible
        };
    }



    //         [Obsolete("Use async version instead")]
    //         public IEnumerable<ContentStatusEntity> GetStatusEntries(string siteId, string contentId)
    //         {
    //             var table = _tableClient.GetTableReference(siteId);
    //             if (!table.Exists()) return Enumerable.Empty<ContentStatusEntity>();

    //             var query = new TableQuery<ContentStatusEntity>()
    //                 .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, contentId));
    //             var statusEntries = table.ExecuteQuery(query);
    //             return statusEntries;
    //         }



    //         [Obsolete("Use async version instead")]
    //         public void AddStatusEntry(string siteId, ContentStatusEntity statusEntry)
    //         {
    //             var table = _tableClient.GetTableReference(siteId);
    //             table.CreateIfNotExists();
    //             var insertOperation = TableOperation.Insert(statusEntry);
    //             table.Execute(insertOperation);
    //         }

    //         public async Task AddStatusEntryAsync(string siteId, ContentStatusEntity statusEntry)
    //         {
    //             var table = _tableClient.GetTableReference(siteId);
    //             await table.CreateIfNotExistsAsync();
    //             var insertOperation = TableOperation.Insert(statusEntry);
    //             await table.ExecuteAsync(insertOperation);
    //         }
}