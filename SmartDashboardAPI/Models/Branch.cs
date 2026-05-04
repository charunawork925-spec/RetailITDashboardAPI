using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDashboardAPI.Models
{
    public class Branch
    {
        [Key]
        [Column("branch_id")] public int BranchId { get; set; }
        [Column("branch_name")] public string BranchName { get; set; } = "";
        [Column("city")] public string City { get; set; } = "";
        [Column("region")] public string Region { get; set; } = "";
        [Column("branch_size_sqft")] public int BranchSizeSqft { get; set; }
        [Column("daily_footfall")] public int DailyFootfall { get; set; }
        [Column("opened_year")] public int OpenedYear { get; set; }
        [Column("manager")]
        public string Manager { get; set; } = "";
    }
}
