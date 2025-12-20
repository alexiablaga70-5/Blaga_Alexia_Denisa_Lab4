using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Blaga_Alexia_Denisa_Lab4.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=TaxiPredictionDb;Trusted_Connection=True;MultipleActiveResultSets=true"
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}