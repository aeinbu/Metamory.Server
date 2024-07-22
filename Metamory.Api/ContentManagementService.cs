using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Metamory.Api.Repositories;

namespace Metamory.Api;


public class ContentManagementService : IDisposable
{
	private readonly IStatusRepository _statusRepository;
	private readonly IContentRepository _contentRepository;
	private readonly VersioningService _versioningService;
	private readonly ICanonicalizeService _canonicalizeService;


	public ContentManagementService(IStatusRepository statusRepository,
		IContentRepository contentRepository,
		VersioningService versioningService,
		ICanonicalizeService canonicalizeService)
	{
		_statusRepository = statusRepository;
		_contentRepository = contentRepository;
		_versioningService = versioningService;
		_canonicalizeService = canonicalizeService;
	}


	public async Task<string> GetCurrentlyPublishedVersionIdAsync(string siteId, string contentId, DateTimeOffset now)
	{
		siteId = _canonicalizeService.Canonicalize(siteId);
		contentId = _canonicalizeService.Canonicalize(contentId);

		var statusEntries = await _statusRepository.GetStatusEntriesAsync(siteId, contentId);
		var publishedVersionId = _versioningService.GetCurrentlyPublishedVersion(now, statusEntries, out var _, out var _);
		return publishedVersionId;
	}


	public async Task<string> DownloadPublishedContentToStreamAsync(string siteId, string contentId, DateTimeOffset now, Stream target)
	{
		var publishedVersionId = await GetCurrentlyPublishedVersionIdAsync(siteId, contentId, now);
		if (publishedVersionId == null) return null;

		var contentType = await DownloadContentToStreamAsync(siteId, contentId, publishedVersionId, target);
		return contentType;
	}


	public async Task ChangeStatusForContentAsync(string siteId, string contentId, string versionId,
		string status, string responsible, DateTimeOffset now, DateTimeOffset? startDate)
	{
		siteId = _canonicalizeService.Canonicalize(siteId);
		contentId = _canonicalizeService.Canonicalize(contentId);
		versionId = _canonicalizeService.Canonicalize(versionId);

		var statusEntry = _statusRepository.CreateContentStatusEntity(contentId, now, startDate ?? now, versionId, status, responsible);

		await _statusRepository.AddStatusEntryAsync(siteId, statusEntry);
	}


	public async Task<IEnumerable<ListContentEntry>> ListContentAsync(string siteId,
	ListContentOptions contentOptions, ListVersionOptions versionOptions)
	{
		siteId = _canonicalizeService.Canonicalize(siteId);

		bool Filter(ListContentEntry entry)
		{
			if (contentOptions.HasFlag(ListContentOptions.AllContent))
				return true;

			if (contentOptions.HasFlag(ListContentOptions.UnpublishedContent) && entry.ContentMetadata.PublishedVersionId == null)
				return true;
			
			if (contentOptions.HasFlag(ListContentOptions.PublishedContent) && entry.ContentMetadata.PublishedVersionId != null)
				return true;

			throw new NotImplementedException();
		}

		var contentList = await _contentRepository.ListContentAsync(siteId);
		var entries = await Task.WhenAll(contentList.Select(async contentId => new ListContentEntry
		{
			ContentId = contentId,
			ContentMetadata = await GetVersionsAsync(siteId, contentId, versionOptions)
		}));
		return entries.Where(Filter);
	}


	public async Task<ContentMetadata> GetVersionsAsync(string siteId, string contentId, ListVersionOptions versionOptions)
	{
		siteId = _canonicalizeService.Canonicalize(siteId);
		contentId = _canonicalizeService.Canonicalize(contentId);

		var versions = await _contentRepository.GetVersionsAsync(siteId, contentId);
		var statusEntries = await _statusRepository.GetStatusEntriesAsync(siteId, contentId);

		var now = DateTimeOffset.Now;
		var publishedVersionId = _versioningService.GetCurrentlyPublishedVersion(now, statusEntries, out var allPreviouslyPublishedVersions, out var allFuturePublishedVersions);
		var allPublishedVersions = new List<string> { publishedVersionId }
			.Concat(allPreviouslyPublishedVersions)
			.Concat(allFuturePublishedVersions);
		var latestVersionId = versions.MaxBy(version => version.Timestamp).VersionId;

		bool Filter(ContentMetadataEntity version)
		{
			if (versionOptions.HasFlag(ListVersionOptions.AllVersions))
				return true;

			if (versionOptions.HasFlag(ListVersionOptions.LatestVersion))
				return version.VersionId == latestVersionId;

			if (versionOptions.HasFlag(ListVersionOptions.AllUnpublishedVersions))
				return allPublishedVersions.Contains(version.VersionId) == false;

			if (versionOptions.HasFlag(ListVersionOptions.CurrentlyPublishedVersion))
				return version.VersionId == publishedVersionId;

			if (versionOptions.HasFlag(ListVersionOptions.AllPreviouslyPublishedVersions))
				return allPreviouslyPublishedVersions.Contains(version.VersionId);

			if (versionOptions.HasFlag(ListVersionOptions.AllFuturePublishedVersions))
				return allFuturePublishedVersions.Contains(version.VersionId);

			throw new NotImplementedException();
		}

		return new ContentMetadata
		{
			Versions = from version in versions
					   where Filter(version)
					   orderby version.Timestamp
					   select new ContentMetadata.Version
					   {
						   VersionId = version.VersionId,
						   PreviousVersionId = version.PreviousVersionId,
						   Timestamp = version.Timestamp,
						   Author = version.Author,
						   Label = version.Label,
					   },
			PublishedVersionId = publishedVersionId
		};
	}


	public async Task<string> DownloadContentToStreamAsync(string siteId, string contentId, string versionId, Stream target)
	{
		siteId = _canonicalizeService.Canonicalize(siteId);
		contentId = _canonicalizeService.Canonicalize(contentId);
		versionId = _canonicalizeService.Canonicalize(versionId);

		var contentType = await _contentRepository.DownloadContentToStreamAsync(siteId, contentId, versionId, target);
		//TODO: Throw an appropriate exception for File Not Found

		return contentType;
	}


	public async Task<ContentMetadata.Version> StoreAsync(string siteId,
		string contentId,
		DateTimeOffset now,
		Stream contentStream,
		string contentType,
		string previousVersionId = null,
		string author = null,
		string label = null)
	{
		siteId = _canonicalizeService.Canonicalize(siteId);
		contentId = _canonicalizeService.Canonicalize(contentId);
		previousVersionId = _canonicalizeService.Canonicalize(previousVersionId);

		var versionId = Guid.NewGuid().ToString();

		var status = "Draft";
		var responsible = "N/A"; //TODO: what? who? logged on user? IPrincipal?
		var statusEntry = _statusRepository.CreateContentStatusEntity(contentId, now, now, versionId, status, responsible);
		var t1 = _statusRepository.AddStatusEntryAsync(siteId, statusEntry);

		var t2 = _contentRepository.AddContentAsync(siteId, contentId, versionId, contentStream, contentType, now, previousVersionId, author, label);

		await Task.WhenAll(new[] { t1, t2 });

		return new ContentMetadata.Version
		{
			VersionId = versionId,
			Timestamp = now,
			PreviousVersionId = previousVersionId,
			Author = author,
			Label = label
		};
	}


	public async Task DeleteContent(string siteId, string contentId)
	{
		siteId = _canonicalizeService.Canonicalize(siteId);
		contentId = _canonicalizeService.Canonicalize(contentId);

		// DeleteContent should delete ALL versions of the content.
		// For deleting a single version, consider implementing a DeleteVersion method.
		// For deleting all unpublished versions, consider implementing a Cleanup/Compact/DeleteAllUnpublishedVersions method.

		//TODO: What should a delete do? MarkAsDeleted, archive to zip, or actually delete?
		// Should it be possible to delete content that
		// - has been published? (No, maybe archive to zip!)
		// - is currently published? (No!)
		// - has a future publish date? (No?)
		// - was never published, and has no future publish date? (yes, delete or archive to zip)

		throw new NotImplementedException();
	}


	void IDisposable.Dispose()
	{
		// ...
	}
}


public static class AsyncEnumerableExtensions
{
	public static async Task<IEnumerable<T>> AwaitAll<T>(this IEnumerable<Task<T>> tasks)
	{
		return await Task.WhenAll(tasks);
	}
}

