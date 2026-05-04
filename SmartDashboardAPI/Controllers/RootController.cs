using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartDashboardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult Root() =>
       Ok(new { message = "InsightIQ API v3 (.NET Core)", status = "running" });
    }
}
