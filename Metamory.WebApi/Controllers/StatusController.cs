using Microsoft.AspNetCore.Mvc;
using Metamory.WebApi.Utils;
using Microsoft.AspNetCore.Authorization;

namespace Metamory.WebApi.Controllers;


[StopwatchFilter]
[ApiController]
public class StatusController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet, Route("/"), Route("/status")]
    public IActionResult GetStatus()
    {
        return Ok("Status OK");
    }
}
