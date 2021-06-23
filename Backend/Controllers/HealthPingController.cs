using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api")]
    [Produces("application/json")]
    public class HealthPingController : Controller
    {
        [HttpGet("healthping")]
        public IActionResult HealthPing()
        {
            var netCoreVer = Environment.Version;
            var runtimeVer = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            return Ok($"Server is up and running. .NET Core Version : {netCoreVer}, Runtime Version : {runtimeVer} ");
        }
    }
}
