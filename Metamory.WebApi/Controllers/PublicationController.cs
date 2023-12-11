using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Metamory.Api;
using Metamory.WebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Metamory.WebApi.Controllers;


[StopwatchFilter]
[ApiController]
public class PublicationController : ControllerBase
{
    private readonly ContentManagementService _contentManagementService;


    public PublicationController(ContentManagementService contentManagementService)
    {
        _contentManagementService = contentManagementService;
    }


    [AllowAnonymous]
    [HttpGet, Route("content/{siteId}/{contentId}")]
    public async Task<IActionResult> GetPublishedContent(string siteId, string contentId)
    {
        string publishedVersionId = await _contentManagementService.GetCurrentlyPublishedVersionIdAsync(siteId, contentId, DateTimeOffset.Now);

        // //TODO: cache control headers
        // var ifNoneMatchHeader = Request.Headers.IfNoneMatch.SingleOrDefault();
        // if (ifNoneMatchHeader != null
        // 	&& "\"" + publishedVersionId + "\"" == ifNoneMatchHeader.Tag)
        // {
        // 	var notModifiedMessage = new HttpResponseMessage(HttpStatusCode.NotModified);
        // 	return notModifiedMessage;
        // }

        if (publishedVersionId == null)
        {
            return new NotFoundResult();
        }

        var stream = new MemoryStream();
        var contentType = await _contentManagementService.DownloadContentToStreamAsync(siteId, contentId, publishedVersionId, stream);
        stream.Seek(0, SeekOrigin.Begin);

        return new FileStreamResult(stream, contentType) { EntityTag = new EntityTagHeaderValue($@"""{publishedVersionId}""") };
    }
}
