
namespace Metamory.WebApi.Controllers.WebApi;


public class StatusChangeModel
{
	public string Responsible { get; set; }

	public DateTimeOffset? StartDate { get; set; }

	public string Status { get; set; }
}