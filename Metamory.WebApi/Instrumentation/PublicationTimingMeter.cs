using System.Diagnostics;

namespace Metamory.WebApi.Instrumentation;

public class PublicationTimingMeter : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _siteId;
    private readonly string _contentId;
    private readonly PublicationMetrics _publicationMetrics;
    public PublicationMetrics.ResultType ResultType {set; private get;} = 0;

    public PublicationTimingMeter(string siteId, string contentId, PublicationMetrics publicationMetrics)
    {
        _stopwatch = Stopwatch.StartNew();

        _siteId = siteId;
        _contentId = contentId;
        _publicationMetrics = publicationMetrics;

        _publicationMetrics.ContentRequested(siteId, contentId);

    }

    public void Dispose()
    {
        var elapsed = _stopwatch.Elapsed;
        _publicationMetrics.Timed(_siteId, _contentId, elapsed, ResultType);
    }
}