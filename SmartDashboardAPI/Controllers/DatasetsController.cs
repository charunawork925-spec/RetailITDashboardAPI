using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Interface;
using SmartDashboardAPI.Services;

namespace SmartDashboardAPI.Controllers
{
    [Route("api/datasets")]
    [ApiController]
    public class DatasetsController : ControllerBase
    {
        private readonly IDatasetService _svc;
        public DatasetsController(IDatasetService svc) { _svc = svc; }

        [HttpGet]
        public IActionResult Get() => Ok(_svc.GetAllDatasets());
        [HttpGet("datasets/{datasetId}/values/{fieldKey}")]
        public async Task<ActionResult<List<string>>> GetDistinctValues(string datasetId, string fieldKey, int limit = 50)
        {
            var ds = _svc.GetDataset(datasetId);
            if (ds == null)
                return BadRequest($"Unknown dataset: {datasetId}");

            var field = ds.Fields.FirstOrDefault(f => f.Key == fieldKey);
            if (field == null)
                return BadRequest($"Unknown field: {fieldKey}");

            try
            {
                var values = await _svc.GetDistinctValuesAsync(datasetId, fieldKey, limit);
                return Ok(values);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("relationships")]
        public ActionResult<List<RelationshipDto>> GetRelationships()
        {
            var relationships = new List<RelationshipDto>
            {
                new() { Id = "products_transactions", From = "products", Join = "transactions",
                       Type = "LEFT JOIN", On = "products.product_id = transactions.product_id",
                       FromLabel = "Products", JoinLabel = "Transactions" },
                new() { Id = "customers_transactions", From = "customers", Join = "transactions",
                       Type = "LEFT JOIN", On = "customers.customer_id = transactions.customer_id",
                       FromLabel = "Customers", JoinLabel = "Transactions" },
                new() { Id = "branches_transactions", From = "branches", Join = "transactions",
                       Type = "LEFT JOIN", On = "branches.branch_id = transactions.branch_id",
                       FromLabel = "Branches", JoinLabel = "Transactions" }
            };
            return Ok(relationships);
        }
    }
}
