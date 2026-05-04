using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDashboardAPI.Models
{
    public class Promotion
    {
        [Key]
        [Column("promo_id")] public int PromoId { get; set; }
        [Column("promo_name")] public string PromoName { get; set; } = "";
        [Column("promo_type")] public string PromoType { get; set; } = "";
        [Column("target_segment")] public string TargetSegment { get; set; } = "";
        [Column("start_date")] public DateOnly StartDate { get; set; }
        [Column("end_date")] public DateOnly EndDate { get; set; }
        [Column("discount_pct")] public decimal DiscountPct { get; set; }
        [Column("redemptions")] public int Redemptions { get; set; }
        [Column("revenue_uplift_lkr")] public decimal RevenueUpliftLkr { get; set; }
        [Column("roi_pct")] public decimal RoiPct { get; set; }
        [Column("dept_id")] public int? DeptId { get; set; }
    }
}
