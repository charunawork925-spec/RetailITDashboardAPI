using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDashboardAPI.Models
{
    public class Supplier
    {
        [Key]
        [Column("supplier_id")] public int SupplierId { get; set; }
        [Column("supplier_name")] public string SupplierName { get; set; } = "";
        [Column("origin_country")] public string OriginCountry { get; set; } = "";
        [Column("halal_certified")] public bool HalalCertified { get; set; }
        [Column("organic_certified")] public bool OrganicCertified { get; set; }
        [Column("lead_time_days")] public int LeadTimeDays { get; set; }
        [Column("on_time_delivery_pct")] public decimal OnTimeDeliveryPct { get; set; }
        [Column("total_orders_ytd")] public int TotalOrdersYtd { get; set; }
        [Column("spend_ytd")] public decimal SpendYtd { get; set; }
    }
}
