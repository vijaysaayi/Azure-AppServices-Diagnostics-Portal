using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api")]
    [Produces("application/json")]
    public class HealthCheckController : Controller
    {
        private IHealthCheckService _healthCheckService;
        public HealthCheckController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet("healthping")]
        public async Task<IActionResult> HealthPing()
        {
            var netCoreVer = Environment.Version;
            var runtimeVer = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            try
            {
                await _healthCheckService.RunHealthCheck();
                return Ok($"Server is up and running. .NET Core Version : {netCoreVer}, Runtime Version : {runtimeVer} ");
            }
            catch (Exception ex)
            {
                return NotFound($"HealthCheck Failed: {ex.Message}");
            }

        }
    }
}
