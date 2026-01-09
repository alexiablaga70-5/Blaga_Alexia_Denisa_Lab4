using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blaga_Alexia_Denisa_Lab4.Data;
using Blaga_Alexia_Denisa_Lab4.Models;


namespace Blaga_Alexia_Denisa_Lab4.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public DbSet<PredictionHistory> PredictionHistories { get; set; }
    }
}