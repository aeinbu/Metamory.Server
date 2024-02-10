using System.Diagnostics.Metrics;

namespace Metamory.WebApi.Instrumentation;

public class PublicationMetrics
{
    private readonly Histogram<double> _timeTakenPerContentRequest;
    private readonly Counter<double> _totalTimeTakenForContentRequests;
    private readonly Counter<int> _requestCounter;
    private readonly Counter<int> _contentServedCounter;
    private readonly Counter<int> _contentNotFoundCounter;

    public PublicationMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Metamory.WebApi");
        _timeTakenPerContentRequest = meter.CreateHistogram<double>("publication.contentServed.timeTaken", "s", "Time taken to serve each content");
        _totalTimeTakenForContentRequests = meter.CreateCounter<double>("publication.contentServed.totalTimeTaken", "s", "Total time taken to serve content");
        _requestCounter = meter.CreateCounter<int>("publication.contentRequested.counter", "number", "Total number of requests");
        _contentServedCounter = meter.CreateCounter<int>("publication.contentServed.counter", "number", "Total number of content found");
        _contentNotFoundCounter = meter.CreateCounter<int>("publication.contentNotFound.counter", "number", "Total number of content not found");
    }

    public void Timed(string siteId, string contentId, TimeSpan timeTaken)
    {
        var timeTakenInSeconds = timeTaken.TotalSeconds;
        _timeTakenPerContentRequest.Record(timeTakenInSeconds,
            new KeyValuePair<string, object>("siteId", siteId),
            new KeyValuePair<string, object>("contentId", contentId));

        _totalTimeTakenForContentRequests.Add(timeTakenInSeconds,
            new KeyValuePair<string, object>("siteId", siteId),
            new KeyValuePair<string, object>("contentId", contentId));
    }

    public void ContentRequested(string siteId, string contentId)
    {
        _requestCounter.Add(1,
            new KeyValuePair<string, object>("siteId", siteId),
            new KeyValuePair<string, object>("contentId", contentId));
    }

    public void ContentServed(string siteId, string contentId)
    {
        _contentServedCounter.Add(1,
            new KeyValuePair<string, object>("siteId", siteId),
            new KeyValuePair<string, object>("contentId", contentId));
    }

    public void ContentNotFound(string siteId, string contentId)
    {
        _contentNotFoundCounter.Add(1,
            new KeyValuePair<string, object>("siteId", siteId),
            new KeyValuePair<string, object>("contentId", contentId));
    }

}
