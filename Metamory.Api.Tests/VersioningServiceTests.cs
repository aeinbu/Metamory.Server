using System;
using Metamory.Api.Repositories;
using NUnit.Framework;

namespace Metamory.Api.Tests;


[TestFixture]
public class VersioningServiceTests
{
	private VersioningService _versioningSvc;

	[SetUp]
	public void SetUp()
	{
		_versioningSvc = new VersioningService();
	}

	[Test]
	public void GetCurrentlyPublishedVersion_NoRows_ReturnsNull()
	{
		var dtNow = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var statusEntries = Array.Empty<TestContentStatusEntity>();

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.Null);
	}

	[Test]
	public void GetCurrentlyPublishedVersion_OneRowNonePublished_ReturnsNull()
	{
		var dt = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dtNow = dt.AddYears(1);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt, StartTime = dt, VersionId = "1", Status = "Ready for review"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.Null);
	}

	[Test]
	public void GetCurrentlyPublishedVersion_TwoRowsNonePublished_ReturnsNull()
	{
		var dt1 = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dt2 = dt1.AddHours(1);
		var dtNow = dt1.AddYears(1);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt1, StartTime = dt1, VersionId = "1", Status = "Ready for review"},
			new TestContentStatusEntity{Timestamp = dt2, StartTime = dt2, VersionId = "2", Status = "Ready for review"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.Null);
	}

	[Test]
	public void GetCurrentlyPublishedVersion_OneRowPublished_ReturnsVersion()
	{
		var dt = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dtNow = dt.AddYears(1);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt, StartTime = dt, VersionId = "1", Status = "Published"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.EqualTo("1"));
	}

	[Test]
	public void GetCurrentlyPublishedVersion_OneRowsPublishedInTheFuture_ReturnsNull()
	{
		var dt = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dtNow = dt.AddYears(-1);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt, StartTime = dt, VersionId = "1", Status = "Published"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.Null);
	}

	[Test]
	public void GetCurrentlyPublishedVersion_TwoRowsOnePublished_ReturnsVersion()
	{
		var dt1 = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dt2 = dt1.AddHours(1);
		var dtNow = dt1.AddYears(1);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt1, StartTime = dt1, VersionId = "1", Status = "Ready for review"},
			new TestContentStatusEntity{Timestamp = dt2, StartTime = dt2, VersionId = "1", Status = "Published"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.EqualTo("1"));
	}

	[Test]
	public void GetCurrentlyPublishedVersion_TwoRowsOnePublishedInTheFuture_ReturnsNull()
	{
		var dt1 = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dt2 = dt1.AddHours(1);
		var dtNow = dt1.AddMinutes(30);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt1, StartTime = dt1, VersionId = "1", Status = "Ready for review"},
			new TestContentStatusEntity{Timestamp = dt2, StartTime = dt2, VersionId = "1", Status = "Published"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.Null);
	}

	[Test]
	public void GetCurrentlyPublishedVersion_TwoRowsTwoPublished_ReturnsVersion()
	{
		var dt1 = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dt2 = dt1.AddHours(1);
		var dtNow = dt1.AddYears(1);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt1, StartTime = dt1, VersionId = "1", Status = "Published"},
			new TestContentStatusEntity{Timestamp = dt2, StartTime = dt2, VersionId = "2", Status = "Published"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.EqualTo("2"));
	}

	[Test]
	public void GetCurrentlyPublishedVersion_TwoRowsTwoPublishedOneInTheFuture_ReturnsVersion()
	{
		var dt1 = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dt2 = dt1.AddHours(1);
		var dtNow = dt1.AddMinutes(30);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt1, StartTime = dt1, VersionId = "1", Status = "Published"},
			new TestContentStatusEntity{Timestamp = dt2, StartTime = dt2, VersionId = "2", Status = "Published"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.EqualTo("1"));
	}

	[Test]
	public void GetCurrentlyPublishedVersion_ThreeRowsMiddleOnePublished_ReturnsVersion()
	{
		var dt1 = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dt2 = dt1.AddHours(1);
		var dt3 = dt1.AddHours(2);
		var dtNow = dt1.AddYears(1);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt1, StartTime = dt1, VersionId = "1", Status = "Ready for review"},
			new TestContentStatusEntity{Timestamp = dt2, StartTime = dt2, VersionId = "1", Status = "Published"},
			new TestContentStatusEntity{Timestamp = dt3, StartTime = dt3, VersionId = "2", Status = "Ready for review"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.EqualTo("1"));
	}

	[Test]
	public void GetCurrentlyPublishedVersion_FirstPublishedThenUnpublished_ReturnsNull()
	{
		var dt1 = new DateTimeOffset(2015, 1, 1, 7, 0, 0, TimeSpan.FromHours(-2));
		var dt2 = dt1.AddHours(1);
		var dtNow = dt1.AddYears(1);
		var statusEntries = new[]
		{
			new TestContentStatusEntity{Timestamp = dt1, StartTime = dt1, VersionId = "1", Status = "Published"},
			new TestContentStatusEntity{Timestamp = dt2, StartTime = dt2, VersionId = "1", Status = "Ready for review"}
		};

		var currentVersion = _versioningSvc.GetCurrentlyPublishedVersion(dtNow, statusEntries, out var _, out var _);

		Assert.That(currentVersion, Is.Null);
	}
}

public class TestContentStatusEntity : IContentStatusEntity
{
	public DateTimeOffset? Timestamp { get; set; }
	public string ContentId { get; set; }
	public string VersionId { get; set; }
	public DateTimeOffset StartTime { get; set; }
	public string Status { get; set; }
	public string Responsible { get; set; }
}
