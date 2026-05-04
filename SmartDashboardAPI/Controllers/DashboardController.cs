using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _svc;
        public DashboardController(IDashboardService svc) { _svc = svc; }

        [HttpGet("charts")]
        public async Task<IActionResult> Charts()
        {
            try { return Ok(await _svc.GetDashboardChartsAsync()); }
            catch (Exception ex) { return StatusCode(500, new { detail = ex.Message }); }
        }
    }
}
