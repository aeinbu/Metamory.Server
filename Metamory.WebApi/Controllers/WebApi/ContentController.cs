﻿using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Metamory.Api;
using Metamory.WebApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Metamory.WebApi.Controllers.WebApi;


[StopwatchFilter]
[Authorize(Policy = AuthPolicies.SiteIdClaim, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class ContentController : ControllerBase
{
    private readonly ContentManagementService _contentManagementService;


    public ContentController(ContentManagementService contentManagementService)
    {
        _contentManagementService = contentManagementService;
    }


    [Authorize(Policy = AuthPolicies.ReviewerRole)]
    [HttpGet, Route("content/{siteId}/{contentId}/versions")]
    public async Task<IActionResult> GetVersions(string siteId, string contentId)
    {
        try
        {
            var versions = await _contentManagementService.GetVersionsAsync(siteId, contentId);
            return Ok(versions);
        }
        catch (Exception)
        {
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
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


    [Authorize(Policy = AuthPolicies.ReviewerRole)]
    [HttpGet, Route("content/{siteId}/{contentId}/{versionId}")]
    public async Task<IActionResult> GetContent(string siteId, string contentId, string versionId)
    {
        var stream = new MemoryStream();
        var contentType = await _contentManagementService.DownloadContentToStreamAsync(siteId, contentId, versionId, stream);
        stream.Seek(0, SeekOrigin.Begin);

        if (contentType == null)
        {
            return new NotFoundResult();
        }

        return new FileStreamResult(stream, contentType);
    }


    [Authorize(Policy = AuthPolicies.EditorRole)]
    [HttpPost, Route("content/{siteId}/{contentId}/{versionId}/status")]
    public async Task<IActionResult> PostStatusChange(string siteId, string contentId, string versionId, StatusChangeModel statusModel)
    {
        var now = DateTimeOffset.Now;
        await _contentManagementService.ChangeStatusForContentAsync(siteId, contentId, versionId, statusModel.Status, statusModel.Responsible, now, statusModel.StartDate);

        return await GetVersions(siteId, contentId);
    }


    // [Authorize(Policy = AuthPolicies.EditorRole)]
    // [HttpDelete, Route("content/{siteId}/{contentId}")]
    // public IHttpActionResult Delete(string siteId, string contentId)
    // {
    // 	_contentVersioningService.DeleteContent(siteId, contentId);
    // 	return StatusCode(HttpStatusCode.NoContent);
    // }
}
