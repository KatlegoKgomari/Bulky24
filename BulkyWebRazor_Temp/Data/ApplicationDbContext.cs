using BulkyWebRazor_Temp.Models;
using Microsoft.EntityFrameworkCore; // must say this so don't have errors

namespace BulkyWebRazor_Temp.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) //whatever configuration we have here, we want to pss to the db context class
        {
                
        }

        public DbSet<Category> Categories { get; set; } //This creates a table in te database called Categories

        protected override void OnModelCreating(ModelBuilder modelBuilder) // Already there in db context and we want to override it. Uisng model builder, we seed data
        {
            //Has data expects an array
            modelBuilder.Entity<Category>().HasData
                ( //On categoty, we want o create these three records 
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Comedy", DisplayOrder =2 },
                new Category { Id = 3, Name = "Drama", DisplayOrder = 3 }
                );
        }
    }
}
