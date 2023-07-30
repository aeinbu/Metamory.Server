using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metamory.Api.Repositories;


public interface IStatusRepository
{
    Task<IEnumerable<IContentStatusEntity>> GetStatusEntriesAsync(string siteId, string contentId);

    Task AddStatusEntryAsync(string siteId, IContentStatusEntity statusEntry);

    IContentStatusEntity CreateContentStatusEntity(string contentId, DateTimeOffset timestamp, DateTimeOffset startTime, string versionId, string status, string responsible);
}