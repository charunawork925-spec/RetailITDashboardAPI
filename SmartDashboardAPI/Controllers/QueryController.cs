using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Controllers
{
    [Route("api/query")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private readonly IQueryService _svc;
        public QueryController(IQueryService svc) { _svc = svc; }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QueryRequest req)
        {
            try { return Ok(await _svc.RunQueryAsync(req)); }
            catch (ArgumentException ex) { return BadRequest(new { detail = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { detail = ex.Message }); }
        }
    }
}
