using BluecoreApi.Models;
using Microsoft.EntityFrameworkCore;
namespace BluecoreApi.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<CreditRequest> CreditRequests => Set<CreditRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("esquema_c");
            modelBuilder.Entity<CreditRequest>().ToTable("credit_cases");
            base.OnModelCreating(modelBuilder);
        }
    }
}
