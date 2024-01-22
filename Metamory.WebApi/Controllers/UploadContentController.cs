using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Metamory.Api;
using Metamory.WebApi.Utils;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Metamory.WebApi.Authorization;

namespace Metamory.WebApi.Controllers;


[StopwatchFilter]
[Authorize(Policy = AuthPolicies.SiteIdClaim, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UploadContentController : ControllerBase
{
	private readonly ContentManagementService _contentManagementService;


	public UploadContentController(ContentManagementService contentManagementService)
	{
		_contentManagementService = contentManagementService;
	}


	[Authorize(Policy = AuthPolicies.ContributorRole)]
	[HttpPost, Route("content/{siteId}/{contentId}")]
	public async Task<IActionResult> Post(string siteId, string contentId, HttpRequestMessage requestMessage)
	{
		var model = this.Request.HasFormContentType
			? await GetPostContentModelFromFormAsync(siteId, contentId)
			: await GetPostContentModelFromAjaxAsync(siteId, contentId);

		if (model.ContentStream != null && model.ContentType != null)
		{
			var contentMetadata = await _contentManagementService.StoreAsync(siteId, contentId, DateTimeOffset.Now,
				model.ContentStream, model.ContentType, model.PreviousVersionId, model.Author, model.Label);
			return Ok(contentMetadata);
		}

		return new StatusCodeResult((int)HttpStatusCode.BadRequest);
	}


	private async Task<PostContentModel> GetPostContentModelFromAjaxAsync(string siteId, string contentId)
	{
		using var sr = new StreamReader(this.Request.Body);
		string jsonBodyString = await sr.ReadToEndAsync();
		var jsonBody = JObject.Parse(jsonBodyString);

		string GetValue(string key)
		{
			var val = jsonBody[key];
			return val?.ToString();
		}

		var model = new PostContentModel()
		{
			Author = GetValue("author"),
			Label = GetValue("label"),
			PreviousVersionId = GetValue("previousVersionId"),
			ContentType = GetValue("contentType"),
			ContentStream = new MemoryStream(Encoding.UTF8.GetBytes(GetValue("content")))
		};

		return model;
	}

	private async Task<PostContentModel> GetPostContentModelFromFormAsync(string siteId, string contentId)
	{
		var formValues = await this.Request.ReadFormAsync();
		string GetValue(string key) => formValues[key];

		var model = new PostContentModel()
		{
			Author = GetValue("author"),
			Label = GetValue("label"),
			PreviousVersionId = GetValue("previousVersionId"),
			ContentType = GetValue("contentType"),
			ContentStream = new MemoryStream(Encoding.UTF8.GetBytes(GetValue("content")))
		};

		return model;
	}


	public class PostContentModel
	{
		public Stream ContentStream { get; set; }
		public string ContentType { get; set; }
		public string PreviousVersionId { get; set; }
		public string Author { get; set; }
		public string Label { get; set; }
	}
}
