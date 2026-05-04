using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IDatabaseRepository _repo;
        public HealthController(IDatabaseRepository repo) { _repo = repo; }

        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            var ok = await _repo.TestConnectionAsync();
            return ok
                ? Ok(new { status = "ok", database = "connected" })
                : StatusCode(500, new { status = "error", database = "unreachable" });
        }
    }
}
