using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDashboardAPI.Models
{
    public class InventoryItem
    {
        [Key]
        [Column("inv_id")] public int InvId { get; set; }
        [Column("product_id")] public int ProductId { get; set; }
        [Column("sku")] public string Sku { get; set; } = "";
        [Column("warehouse_zone")] public string WarehouseZone { get; set; } = "";
        [Column("quantity_on_hand")] public int QuantityOnHand { get; set; }
        [Column("quantity_on_order")] public int QuantityOnOrder { get; set; }
        [Column("days_of_supply")] public int DaysOfSupply { get; set; }
        [Column("shelf_life_days")] public int ShelfLifeDays { get; set; }
        [Column("expiry_risk_units")] public int ExpiryRiskUnits { get; set; }
        [Column("last_received_date")] public DateOnly LastReceivedDate { get; set; }
    }
}
