using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Models
{
    public class ShopContext: DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public ShopContext(DbContextOptions<ShopContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasMany(p => p.Products).WithOne(c => c.Category).HasForeignKey(a=>a.CategoryId);

            modelBuilder.Seed();
        }
    }
}
