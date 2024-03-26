using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Metamory.Api;
using Metamory.WebApi.Instrumentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Metamory.WebApi.Controllers;


[ApiController]
public class PublicationController : ControllerBase
{
    private readonly ContentManagementService _contentManagementService;
    private readonly PublicationMetrics _publicationMetrics;

    public PublicationController(ContentManagementService contentManagementService, PublicationMetrics publicationMetrics)
    {
        _contentManagementService = contentManagementService;
        _publicationMetrics = publicationMetrics;
    }


    [AllowAnonymous]
    // [StopwatchFilter]
    [HttpGet, Route("content/{siteId}/{contentId}")]
    public async Task<IActionResult> GetPublishedContent(string siteId, string contentId)
    {
        using (var timer = new PublicationTimingMeter(siteId, contentId, _publicationMetrics))
        {
            // _publicationMetrics.ContentRequested(siteId, contentId);

            string publishedVersionId = await _contentManagementService.GetCurrentlyPublishedVersionIdAsync(siteId, contentId, DateTimeOffset.Now);

            // //TODO: cache control headers
            // var ifNoneMatchHeader = Request.Headers.IfNoneMatch.SingleOrDefault();
            // if (ifNoneMatchHeader != null
            // 	&& "\"" + publishedVersionId + "\"" == ifNoneMatchHeader.Tag)
            // {
            // 	var notModifiedMessage = new HttpResponseMessage(HttpStatusCode.NotModified);
            //     timer.ResultType = PublicationMetrics.ResultType.ContentCached;
            // 	return notModifiedMessage;
            // }

            if (publishedVersionId == null)
            {
                timer.ResultType = PublicationMetrics.ResultType.ContentNotFound;
                return new NotFoundResult();
            }

            var stream = new MemoryStream();
            var contentType = await _contentManagementService.DownloadContentToStreamAsync(siteId, contentId, publishedVersionId, stream);
            stream.Seek(0, SeekOrigin.Begin);

            timer.ResultType = PublicationMetrics.ResultType.ContentServed;
            return new FileStreamResult(stream, contentType) { EntityTag = new EntityTagHeaderValue($@"""{publishedVersionId}""") };
        }
    }
}
