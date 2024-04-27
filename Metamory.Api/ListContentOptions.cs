namespace Metamory.Api;


[Flags]
public enum ListContentOptions
{
	UnpublishedContent = 0x01,
	PublishedContent = 0x02,
	AllContent = UnpublishedContent | PublishedContent
}

