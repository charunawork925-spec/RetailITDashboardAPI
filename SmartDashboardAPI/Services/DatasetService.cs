using Npgsql;
using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Services
{
    public class DatasetService : IDatasetService
    {
        private readonly IDatabaseRepository _repository;

        private static readonly List<DatasetMeta> _datasets = BuildDatasets();

        public DatasetService(IDatabaseRepository repository)
        {
            _repository = repository;
        }
        public List<DatasetMeta> GetAllDatasets() => _datasets;

        public DatasetMeta? GetDataset(string id) => _datasets.FirstOrDefault(d => d.Id == id);

        private static List<FilterOption> Opts(params object[] values)
         => values.Select(v => new FilterOption(v.ToString()!, v)).ToList();

        private static List<DatasetMeta> BuildDatasets() =>
        [
            new("customers","Customers","👥","customers",["transactions"],
            [
                new("gender",             "Gender",               "dim",  "gender"),
                new("age_group",          "Age Group",            "dim",  "age_group"),
                new("religion",           "Religion",             "dim",  "religion"),
                new("area",               "Area",                 "dim",  "area"),
                new("loyalty_tier",       "Loyalty Tier",         "dim",  "loyalty_tier"),
                new("preferred_category", "Preferred Category",   "dim",  "preferred_category"),
                new("avg_cart_size_lkr",  "Avg Cart Size (LKR)",  "msr",  "avg_cart_size_lkr"),
                new("visits_per_month",   "Visits / Month",       "msr",  "visits_per_month"),
                new("total_spend_ytd",    "Total Spend YTD (LKR)","msr",  "total_spend_ytd"),
                new("join_date",          "Join Date",            "date", "join_date"),
            ],
            ["loyalty_tier","gender","religion","area","age_group"],
            new()
            {
                ["loyalty_tier"] = Opts("Gold","Silver","Bronze","Non-Member"),
                ["gender"]       = Opts("Male","Female"),
                ["religion"]     = Opts("Buddhist","Muslim","Christian","Hindu"),
                ["area"]         = Opts("Colombo","Kandy","Galle","Negombo","Matara","Kurunegala","Jaffna","Batticaloa"),
                ["age_group"]    = Opts("18-25","26-35","36-50","50+"),
            }),

        new("products","Products","📦","products",["transactions"],
            [
                new("product_name",          "Product Name",        "dim",  "product_name"),
                new("category",              "Category",            "dim",  "category"),
                new("sub_category",          "Sub-Category",        "dim",  "sub_category"),
                new("brand",                 "Brand",               "dim",  "brand"),
                new("is_halal",              "Halal Certified",     "bool", "is_halal"),
                new("is_organic",            "Organic",             "bool", "is_organic"),
                new("is_local",              "Locally Sourced",     "bool", "is_local"),
                new("avg_price_lkr",         "Avg Price (LKR)",     "msr",  "avg_price_lkr"),
                new("cost_price_lkr",        "Cost Price (LKR)",    "msr",  "cost_price_lkr"),
                new("gross_profit_per_unit", "GP per Unit (LKR)",   "msr",  "gross_profit_per_unit"),
                new("gp_margin_pct",         "GP Margin %",         "msr",  "gp_margin_pct"),
                new("units_sold_ytd",        "Units Sold",          "msr",  "units_sold_ytd"),
                new("revenue_ytd",           "Revenue (LKR)",       "msr",  "revenue_ytd"),
                new("gross_profit_ytd",      "Gross Profit (LKR)",  "msr",  "gross_profit_ytd"),
                new("stock_level",           "Stock Level",         "msr",  "stock_level"),
                new("reorder_point",         "Reorder Point",       "msr",  "reorder_point"),
            ],
            ["category","is_halal","is_organic","brand"],
            new()
            {
                ["category"] = Opts("Dairy & Eggs","Vegetables","Meat & Poultry","Bakery & Bread","Beverages","Dry Goods","Frozen","Cooking Oils","Snacks","Personal Care","Household","Organic & Health"),
                ["is_halal"] = Opts(true, false),
                ["is_organic"] = Opts(true, false),
            }),

        new("transactions","Transactions","🧾","transactions",["products","customers","branches"],
            [
                new("txn_month",      "Month",             "date", "txn_month"),
                new("day_of_week",    "Day of Week",       "dim",  "TRIM(day_of_week)"),
                new("time_of_day",    "Time of Day",       "dim",  "time_of_day"),
                new("payment_method", "Payment Method",    "dim",  "payment_method"),
                new("total_value_lkr","Revenue (LKR)",     "msr",  "total_value_lkr"),
                new("discount_lkr",   "Discount (LKR)",    "msr",  "discount_lkr"),
                new("num_items",      "No. of Items",      "msr",  "num_items"),
                new("basket_count",   "Basket Count",      "msr",  "COUNT(transactions.txn_id)"),
            ],
            ["payment_method","time_of_day"],
            new()
            {
                ["payment_method"] = Opts("Cash","Card","QR Pay","Voucher"),
                ["time_of_day"]    = Opts("Morning","Afternoon","Evening","Night"),
            }),

        new("departments","Departments","🏬","departments",[],
            [
                new("dept_name",      "Department",           "dim","dept_name"),
                new("floor_zone",     "Floor / Zone",         "dim","floor_zone"),
                new("manager",        "Manager",              "dim","manager"),
                new("staff_count",    "Staff Count",          "msr","staff_count"),
                new("revenue_target", "Revenue Target (LKR)", "msr","revenue_target"),
                new("actual_revenue", "Actual Revenue (LKR)", "msr","actual_revenue"),
                new("shrinkage_pct",  "Shrinkage %",          "msr","shrinkage_pct"),
            ],
            ["dept_name"],
            new() { ["dept_name"] = Opts("Dairy & Eggs","Fresh Vegetables","Meat & Poultry","Bakery & Bread","Beverages","Dry Goods & Cereal","Frozen Foods","Cooking Oils","Snacks & Confec","Personal Care","Household Items","Organic & Health") }),

        new("branches","Branches","📍","branches",["transactions"],
            [
                new("branch_name",      "Branch Name",    "dim",  "branch_name"),
                new("city",             "City",           "dim",  "city"),
                new("region",           "Region",         "dim",  "region"),
                new("manager",          "Manager",        "dim",  "manager"),
                new("branch_size_sqft", "Size (sqft)",    "msr",  "branch_size_sqft"),
                new("daily_footfall",   "Daily Footfall", "msr",  "daily_footfall"),
                new("opened_year",      "Year Opened",    "date", "opened_year"),
            ],
            ["city","region"],
            new()
            {
                ["city"]   = Opts("Colombo","Kandy","Galle","Negombo","Matara","Kurunegala","Jaffna","Batticaloa"),
                ["region"] = Opts("Western","Central","Southern","North Western","Northern","Eastern"),
            }),

        new("suppliers","Suppliers","🚛","suppliers",[],
            [
                new("supplier_name",        "Supplier Name",      "dim",  "supplier_name"),
                new("origin_country",       "Origin Country",     "dim",  "origin_country"),
                new("halal_certified",      "Halal Certified",    "bool", "halal_certified"),
                new("organic_certified",    "Organic Certified",  "bool", "organic_certified"),
                new("lead_time_days",       "Lead Time (days)",   "msr",  "lead_time_days"),
                new("on_time_delivery_pct", "On-Time Delivery %", "msr",  "on_time_delivery_pct"),
                new("total_orders_ytd",     "Total Orders YTD",   "msr",  "total_orders_ytd"),
                new("spend_ytd",            "Spend YTD (LKR)",    "msr",  "spend_ytd"),
            ],
            ["origin_country","halal_certified"],
            new()
            {
                ["origin_country"]  = Opts("Sri Lanka","New Zealand","Switzerland","United Kingdom","Denmark","Malaysia","Thailand"),
                ["halal_certified"] = Opts(true, false),
            }),

        new("promotions","Promotions","🎯","promotions",[],
            [
                new("promo_name",         "Promotion Name",       "dim",  "promo_name"),
                new("promo_type",         "Promo Type",           "dim",  "promo_type"),
                new("target_segment",     "Target Segment",       "dim",  "target_segment"),
                new("start_date",         "Start Date",           "date", "start_date"),
                new("end_date",           "End Date",             "date", "end_date"),
                new("discount_pct",       "Discount %",           "msr",  "discount_pct"),
                new("redemptions",        "Redemptions",          "msr",  "redemptions"),
                new("revenue_uplift_lkr", "Revenue Uplift (LKR)", "msr",  "revenue_uplift_lkr"),
                new("roi_pct",            "ROI %",                "msr",  "roi_pct"),
            ],
            ["promo_type","target_segment"],
            new()
            {
                ["promo_type"]     = Opts("Bundle Deal","Seasonal Sale","Loyalty Reward","BOGO","Category Sale","Event Sale","Clearance","Time-Limited","Regional","Grand Opening"),
                ["target_segment"] = Opts("Muslim Customers","All Customers","Gold Members","Silver Members","Health Conscious","Female Customers","New Members","Families"),
            }),

        new("inventory","Inventory","🏪","inventory",[],
            [
                new("sku",               "SKU",                "dim",  "sku"),
                new("warehouse_zone",    "Warehouse Zone",     "dim",  "warehouse_zone"),
                new("quantity_on_hand",  "Qty On Hand",        "msr",  "quantity_on_hand"),
                new("quantity_on_order", "Qty On Order",       "msr",  "quantity_on_order"),
                new("days_of_supply",    "Days of Supply",     "msr",  "days_of_supply"),
                new("shelf_life_days",   "Shelf Life (days)",  "msr",  "shelf_life_days"),
                new("expiry_risk_units", "Expiry Risk Units",  "msr",  "expiry_risk_units"),
                new("last_received_date","Last Received",      "date", "last_received_date"),
            ],
            ["warehouse_zone"],
            new() { ["warehouse_zone"] = Opts("Zone-A","Zone-B","Zone-C","Zone-D","Zone-E","Cold-1","Cold-2") }),
    ];

        //public async Task<List<string>> GetDistinctValuesAsync(string datasetId, string fieldKey, int limit = 50)
        //{
        //    var ds = GetDataset(datasetId);
        //    if (ds == null)
        //        return new List<string>();

        //    var field = ds.Fields.FirstOrDefault(f => f.Key == fieldKey);
        //    if (field == null)
        //        return new List<string>();

        //    return await _repository.GetDistinctValuesAsync(ds.TableName, field.Column, limit);
        //}


        //public async Task<List<string>> GetDistinctValues(string datasetId, string column, int limit)
        //{
        //    var ds = _datasets.FirstOrDefault(d => d.Id == datasetId);
        //    if (ds == null)
        //        return new List<string>();

        //    var sql = $"SELECT DISTINCT CAST({column} AS TEXT) AS val FROM {ds.Table} WHERE {column} IS NOT NULL ORDER BY val LIMIT @limit";

        //    using var connection = new NpgsqlConnection(_c);
        //    var result = await connection.QueryAsync<string>(sql, new { limit = Math.Min(limit, 200) });
        //    return result.ToList();
        //}

        public async Task<List<string>> GetDistinctValuesAsync(string datasetId, string fieldKey, int limit = 50)
        {
            var ds = GetDataset(datasetId);
            if (ds == null)
                return new List<string>();

            var field = ds.Fields.FirstOrDefault(f => f.Key == fieldKey);
            if (field == null)
                return new List<string>();

            return await _repository.GetDistinctValuesAsync(ds.Table, field.Column, limit);
        }

    }
}
