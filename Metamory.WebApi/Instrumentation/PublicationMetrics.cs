using System.Diagnostics.Metrics;

namespace Metamory.WebApi.Instrumentation;

public class PublicationMetrics
{
    public enum ResultType { ContentServed = 1, ContentNotFound = 2 };

    public const string MeterName = "Metamory.WebApi";
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<int> _requestCounter;

    public PublicationMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _requestDuration = meter.CreateHistogram<double>("publication.contentServed.duration", "ms", "Time taken to serve each content");
        _requestCounter = meter.CreateCounter<int>("publication.contentRequested.counter", "", "Total number of requests");
    }

    public void ContentRequested(string siteId, string contentId)
    {
        _requestCounter.Add(1,
            new KeyValuePair<string, object>("siteId", siteId),
            new KeyValuePair<string, object>("contentId", contentId));
    }

    public void Timed(string siteId, string contentId, TimeSpan timeTaken, ResultType resultType)
    {
        var timeTakenInSeconds = timeTaken.TotalMilliseconds;
        _requestDuration.Record(timeTakenInSeconds,
            new KeyValuePair<string, object>("result", resultType),
            new KeyValuePair<string, object>("siteId", siteId),
            new KeyValuePair<string, object>("contentId", contentId));
    }
}
