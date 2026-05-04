using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Controllers
{
    [Route("api/top10")]
    [ApiController]
    public class Top10Controller : ControllerBase
    {
        private readonly IDashboardService _svc;
        public Top10Controller(IDashboardService svc) { _svc = svc; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try { return Ok(await _svc.GetTop10Async()); }
            catch (Exception ex) { return StatusCode(500, new { detail = ex.Message }); }
        }
    }
}
