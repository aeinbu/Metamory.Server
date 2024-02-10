using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Metamory.WebApi.Instrumentation;


public class StopwatchFilterAttribute : ActionFilterAttribute
{
	private Stopwatch _stopwatch;

	public override void OnActionExecuting(ActionExecutingContext actionContext)
	{
		_stopwatch = Stopwatch.StartNew();
	}

	public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
	{
		var elapsed = _stopwatch.Elapsed;
		actionExecutedContext.HttpContext.Response.Headers.Append("X-Time-Taken", elapsed.ToString("c"));
	}
}