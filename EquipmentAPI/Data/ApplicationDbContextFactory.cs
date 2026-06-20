using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentAPI.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var databasePath = Path.Combine(Directory.GetCurrentDirectory(), "equipment-api.db");

            optionsBuilder.UseSqlite($"Data Source={databasePath}");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
