using Microsoft.EntityFrameworkCore;
using SmartDashboardAPI.Models;
using System.Runtime.CompilerServices;

namespace SmartDashboardAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
              : base(options) { }

        // ── Entity Tables ────────────────────────────────────────
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionItem> TransactionItems { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<InventoryItem> Inventory { get; set; }

        // ── Keyless result sets (stored procedure returns) ───────
        public DbSet<SpQueryResult> SpQueryResults { get; set; }
        public DbSet<SpKpiResult> SpKpiResults { get; set; }
        public DbSet<SpChartRow> SpChartRows { get; set; }
        public DbSet<SpTop10Row> SpTop10Rows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SpQueryResult>()
                .HasNoKey();

            modelBuilder.Entity<SpKpiResult>()
                .HasNoKey();
            modelBuilder.Entity<SpChartRow>()
                .HasNoKey();
            modelBuilder.Entity<SpTop10Row>()
                .HasNoKey();

        }

    }
}
