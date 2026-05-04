using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Controllers
{
    [Route("api/kpis")]
    [ApiController]
    public class KpisController : ControllerBase
    {
        private readonly IKpiService _svc;
        public KpisController(IKpiService svc) { _svc = svc; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try { return Ok(await _svc.GetKpisAsync()); }
            catch (Exception ex) { return StatusCode(500, new { detail = ex.Message }); }
        }
    }
}
