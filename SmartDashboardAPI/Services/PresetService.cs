using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Services
{
    public class PresetService : IPresetService
    {
        public List<object> GetPresets() =>
   [
       new { id="gp_by_product",    name="GP by Product",           icon="💰", color="green",
              description="Revenue vs Gross Profit per product — bar chart",
              tags=new[]{"GP","Products","Revenue"},
              widgets=new[]
              {
                  new { type="bar",  dataset="products", join_dataset=(string?)null, dimensions=new[]{"category"}, measures=new[]{"revenue_ytd","gross_profit_ytd"}, title="Revenue vs GP by Category" },
                  new { type="grid", dataset="products", join_dataset=(string?)null, dimensions=new[]{"product_name","category","brand"}, measures=new[]{"units_sold_ytd","revenue_ytd","gross_profit_ytd","gp_margin_pct"}, title="Product GP Table" }
              }},

        new { id="gp_product_month", name="GP by Product / Month",   icon="📈", color="cyan",
              description="Products JOIN Transactions — GP trend per category over months",
              tags=new[]{"GP","Monthly","Products"},
              widgets=new[]
              {
                  new { type="line", dataset="products", join_dataset="transactions", dimensions=new[]{"category","txn_month"}, measures=new[]{"gross_profit_ytd"}, title="GP by Category by Month" },
                  new { type="bar",  dataset="products", join_dataset=(string?)null,  dimensions=new[]{"product_name"},         measures=new[]{"gross_profit_ytd"}, title="GP by Product" }
              }},

        new { id="revenue_branch_month", name="Revenue by Branch / Month", icon="🏪", color="blue",
              description="Branches JOIN Transactions — revenue line per branch over months",
              tags=new[]{"Branch","Revenue","Monthly"},
              widgets=new[]
              {
                  new { type="line", dataset="branches", join_dataset="transactions", dimensions=new[]{"branch_name","txn_month"}, measures=new[]{"total_value_lkr"},                title="Revenue by Branch by Month" },
                  new { type="bar",  dataset="branches", join_dataset="transactions", dimensions=new[]{"city"},                   measures=new[]{"total_value_lkr","basket_count"}, title="Revenue & Txns by City" }
              }},

        new { id="customer_spend_month", name="Customer Spend / Month", icon="👥", color="purple",
              description="Customers JOIN Transactions — spend by loyalty tier over months",
              tags=new[]{"Customers","Monthly","Loyalty"},
              widgets=new[]
              {
                  new { type="line", dataset="customers", join_dataset="transactions", dimensions=new[]{"loyalty_tier","txn_month"}, measures=new[]{"total_value_lkr"},                title="Spend by Loyalty Tier by Month" },
                  new { type="bar",  dataset="customers", join_dataset="transactions", dimensions=new[]{"loyalty_tier"},             measures=new[]{"total_value_lkr","basket_count"}, title="Spend & Txns by Tier" }
              }},

        new { id="monthly_revenue", name="Monthly Revenue", icon="📅", color="green",
              description="Monthly revenue line with discount on dual axis",
              tags=new[]{"Revenue","Discount","Monthly"},
              widgets=new[]
              {
                  new { type="line", dataset="transactions", join_dataset=(string?)null, dimensions=new[]{"txn_month"},      measures=new[]{"total_value_lkr","discount_lkr"}, title="Revenue vs Discount by Month" },
                  new { type="bar",  dataset="transactions", join_dataset=(string?)null, dimensions=new[]{"payment_method"}, measures=new[]{"basket_count"},                  title="Transactions by Payment" }
              }},

        new { id="department_pl", name="Department P&L", icon="🏬", color="purple",
              description="Actual vs target revenue and shrinkage",
              tags=new[]{"Departments","Revenue","Targets"},
              widgets=new[]
              {
                  new { type="bar",  dataset="departments", join_dataset=(string?)null, dimensions=new[]{"dept_name"},          measures=new[]{"actual_revenue","revenue_target"},    title="Revenue vs Target" },
                  new { type="grid", dataset="departments", join_dataset=(string?)null, dimensions=new[]{"dept_name","manager"}, measures=new[]{"staff_count","revenue_target","actual_revenue","shrinkage_pct"}, title="Dept P&L Table" }
              }},

        new { id="supplier_scorecard", name="Supplier Scorecard", icon="🚛", color="amber",
              description="On-time delivery %, lead time and YTD spend",
              tags=new[]{"Suppliers","Delivery","Spend"},
              widgets=new[]
              {
                  new { type="bar",  dataset="suppliers", join_dataset=(string?)null, dimensions=new[]{"origin_country"},                  measures=new[]{"on_time_delivery_pct","lead_time_days"},       title="Delivery % & Lead Time" },
                  new { type="grid", dataset="suppliers", join_dataset=(string?)null, dimensions=new[]{"supplier_name","origin_country"},   measures=new[]{"lead_time_days","on_time_delivery_pct","spend_ytd"}, title="Supplier Details" }
              }},

        new { id="inventory_risk", name="Inventory Risk", icon="⚠️", color="red",
              description="Warehouse zone stock vs expiry risk",
              tags=new[]{"Inventory","Stock","Expiry"},
              widgets=new[]
              {
                  new { type="bar",  dataset="inventory", join_dataset=(string?)null, dimensions=new[]{"warehouse_zone"},       measures=new[]{"quantity_on_hand","expiry_risk_units"},            title="Stock vs Expiry by Zone" },
                  new { type="grid", dataset="inventory", join_dataset=(string?)null, dimensions=new[]{"sku","warehouse_zone"}, measures=new[]{"quantity_on_hand","days_of_supply","expiry_risk_units"}, title="Inventory Status" }
              }},

        new { id="promotion_roi", name="Promotion ROI", icon="🎯", color="pink",
              description="Redemptions and ROI % by promo type",
              tags=new[]{"Promotions","ROI","Revenue"},
              widgets=new[]
              {
                  new { type="bar",  dataset="promotions", join_dataset=(string?)null, dimensions=new[]{"promo_type"},                              measures=new[]{"redemptions","roi_pct"},                           title="Redemptions & ROI by Type" },
                  new { type="grid", dataset="promotions", join_dataset=(string?)null, dimensions=new[]{"promo_name","promo_type","target_segment"}, measures=new[]{"redemptions","revenue_uplift_lkr","roi_pct"},      title="Promo Scorecard" }
              }},
    ];

    }
}
