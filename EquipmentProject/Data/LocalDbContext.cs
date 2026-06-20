using SharedEquipment.Models;
using Microsoft.EntityFrameworkCore;

namespace EquipmentProject.Data
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        public DbSet<RentalContract> RentalContracts { get; set; }
        public DbSet<RentalDetail> RentalDetails { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SyncLog> SyncLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "equipment.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }

            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        public class SyncLog
        {
            public int Id { get; set; }
            public DateTime LastSyncDate { get; set; }
        }
    }
}
