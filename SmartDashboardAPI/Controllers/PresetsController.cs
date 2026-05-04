using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Controllers
{
    [Route("api/presets")]
    [ApiController]
    public class PresetsController : ControllerBase
    {
        private readonly IPresetService _svc;
        public PresetsController(IPresetService svc) { _svc = svc; }

        [HttpGet]
        public IActionResult Get() => Ok(_svc.GetPresets());
    }
}
