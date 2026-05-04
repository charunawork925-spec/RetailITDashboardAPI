using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDashboardAPI.Models
{
    public class Product
    {
        [Key]
        [Column("product_id")] public int ProductId { get; set; }
        [Column("product_name")] public string ProductName { get; set; } = "";
        [Column("category")] public string Category { get; set; } = "";
        [Column("sub_category")] public string SubCategory { get; set; } = "";
        [Column("brand")] public string Brand { get; set; } = "";
        [Column("is_halal")] public bool IsHalal { get; set; }
        [Column("is_organic")] public bool IsOrganic { get; set; }
        [Column("is_local")] public bool IsLocal { get; set; }
        [Column("avg_price_lkr")] public decimal AvgPriceLkr { get; set; }
        [Column("cost_price_lkr")] public decimal CostPriceLkr { get; set; }
        [Column("gross_profit_per_unit")] public decimal GrossProfitPerUnit { get; set; }
        [Column("gp_margin_pct")] public decimal GpMarginPct { get; set; }
        [Column("units_sold_ytd")] public int UnitsSoldYtd { get; set; }
        [Column("revenue_ytd")] public decimal RevenueYtd { get; set; }
        [Column("gross_profit_ytd")] public decimal GrossProfitYtd { get; set; }
        [Column("stock_level")] public int StockLevel { get; set; }
        [Column("reorder_point")] public int ReorderPoint { get; set; }
        [Column("supplier_id")] public int? SupplierId { get; set; }
        [Column("dept_id")] public int? DeptId { get; set; }
    }
}
