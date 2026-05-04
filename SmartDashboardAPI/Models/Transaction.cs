using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDashboardAPI.Models
{
    public class Transaction
    {
        [Key]
        [Column("customer_id")] public int CustomerId { get; set; }
        [Column("first_name")] public string FirstName { get; set; } = "";
        [Column("last_name")] public string LastName { get; set; } = "";
        [Column("gender")] public string Gender { get; set; } = "";
        [Column("age_group")] public string AgeGroup { get; set; } = "";
        [Column("religion")] public string Religion { get; set; } = "";
        [Column("area")] public string Area { get; set; } = "";
        [Column("loyalty_tier")] public string LoyaltyTier { get; set; } = "";
        [Column("avg_cart_size_lkr")] public decimal AvgCartSizeLkr { get; set; }
        [Column("visits_per_month")] public decimal VisitsPerMonth { get; set; }
        [Column("total_spend_ytd")] public decimal TotalSpendYtd { get; set; }
        [Column("join_date")] public DateOnly JoinDate { get; set; }
        [Column("preferred_category")] public string PreferredCategory { get; set; } = "";
        [Column("branch_id")] public int? BranchId { get; set; }
    }
}
