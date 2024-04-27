using System;
using System.Collections.Generic;
using System.Linq;
using Metamory.Api.Repositories;

namespace Metamory.Api;


public class VersioningService
{
	public string GetCurrentlyPublishedVersion(DateTimeOffset now,
		IEnumerable<IContentStatusEntity> statusEntries,
		out List<string> previouslyPublishedVersions,
		out List<string> futurePublishedVersions)
	{
		previouslyPublishedVersions = new();
		futurePublishedVersions = new();

		var statusEntriesBeforeNow = from entry in statusEntries
									 where entry.StartTime <= now
									 orderby entry.Timestamp
									 select entry;

		string currentlyPublishedVersionId = null;
		foreach (var entry in statusEntriesBeforeNow)
		{
			if (entry.StartTime <= now)
			{
				if (entry.VersionId == currentlyPublishedVersionId)
				{
					previouslyPublishedVersions.Add(currentlyPublishedVersionId);
					currentlyPublishedVersionId = null;
				}
				if (entry.Status == "Published")
				{
					currentlyPublishedVersionId = entry.VersionId;
				}
			}
			else{
				if (entry.Status == "Published")
				{
					futurePublishedVersions.Add(entry.VersionId);
				}
			}
		}

		return currentlyPublishedVersionId;
	}
}
