using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDashboardAPI.Models
{
    public class Department
    {
        [Key]
        [Column("dept_id")] public int DeptId { get; set; }
        [Column("dept_name")] public string DeptName { get; set; } = "";
        [Column("floor_zone")] public string FloorZone { get; set; } = "";
        [Column("manager")] public string Manager { get; set; } = "";
        [Column("staff_count")] public int StaffCount { get; set; }
        [Column("revenue_target")] public decimal RevenueTarget { get; set; }
        [Column("actual_revenue")] public decimal ActualRevenue { get; set; }
        [Column("shrinkage_pct")] public decimal ShrinkagePct { get; set; }
    }
}
