using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metamory.Api.Repositories
{
    public interface IStatusRepository
    {
        Task<IEnumerable<ContentStatusEntity>> GetStatusEntriesAsync(string siteId, string contentId);

        Task AddStatusEntryAsync(string siteId, ContentStatusEntity statusEntry);
    }
}