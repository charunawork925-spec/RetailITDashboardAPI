using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDashboardAPI.Models
{
    public class TransactionItem
    {

        [Key]
        [Column("item_id")] public int ItemId { get; set; }
        [Column("txn_id")] public int TxnId { get; set; }
        [Column("product_id")] public int ProductId { get; set; }
        [Column("quantity")] public int Quantity { get; set; }
        [Column("unit_price_lkr")] public decimal UnitPriceLkr { get; set; }
        [Column("line_total_lkr")] public decimal LineTotalLkr { get; set; }
    }
}
