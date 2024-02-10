using System.Diagnostics;

namespace Metamory.WebApi.Instrumentation;

public class TimeTakenMeter : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _siteId;
    private readonly string _contentId;
    private readonly PublicationMetrics _metamoryMetrics;

    public TimeTakenMeter(string siteId, string contentId, PublicationMetrics metamoryMetrics)
    {
        _stopwatch = Stopwatch.StartNew();

        _siteId = siteId;
        _contentId = contentId;
        _metamoryMetrics = metamoryMetrics;
    }

    public void Dispose()
    {
        var elapsed = _stopwatch.Elapsed;
		_metamoryMetrics.Timed(_siteId, _contentId, elapsed);
    }
}