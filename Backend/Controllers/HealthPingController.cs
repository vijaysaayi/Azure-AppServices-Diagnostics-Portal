using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api")]
    public class HealthPingController : Controller
    {
        [HttpGet("healthping")]
        public IActionResult HealthPing()
        {
            return Ok("App is running");
        }
    }
}
