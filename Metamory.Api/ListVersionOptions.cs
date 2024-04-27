namespace Metamory.Api;


[Flags]
public enum ListVersionOptions
{
	AllUnpublishedVersions = 0x01,
	LatestVersion = 0x02,
	CurrentlyPublishedVersion = 0x04,
	AllPreviouslyPublishedVersions = 0x08,
	AllFuturePublishedVersions = 0x10,
	AllVersions = AllUnpublishedVersions | LatestVersion | CurrentlyPublishedVersion | AllPreviouslyPublishedVersions | AllFuturePublishedVersions
}

