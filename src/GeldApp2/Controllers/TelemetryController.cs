using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;

namespace GeldApp2.Controllers
{
    [Authorize]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("/api/app/version")]
        public ActionResult<string> GetAppVersion()
        {
            var version = typeof(Startup).Assembly
                            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                            .InformationalVersion;

            return this.Ok(new { version });
        }
    }
}
