using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//When we do CRUD operations with data, we will be working in this file

namespace Bulky.DataAccess.Data
{// Now we will be adding something to establish the connection between our database and entity framework.
 // Whatever we do next is basic configuration that is needed for the entity framework

    //It must extend the DbContext class. DbConetxt is the root class of entity framework Core and we'll use it to access entity framework
    // DbContext is a built-in class inside the Entity Framework Core NuGet package
    // Rememeber that the connection string is inside appsettings.json
    // When we configure this ApplicationDbContext, we will get the connection string as a parameter as DBContextOption and that we will be passing on to the bae class
    public class ApplicationDbContext: IdentityDbContext<IdentityUser>
    {
        // We have to pass the connection string into the constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) // we configure DB context options and that will be on the class Application DB context and we call it options
        //whatever configuration we add here, we want to add to the DBContext class. It's kinda like the super in java
        // We register this ApllicationDbContext in Program.cs. Whenever we have to register something, we register it in Program.cs
        {
                
        }

        // When we have to create a table, we have to create someting called DBset inside applicationDBContext
        // The type is dbset and the entity is Category
        public DbSet<Category> Categories { get; set; } // This creates a table in the database. the table is called Categories
        public DbSet<Product> Products { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // We override this method that is found in DbContext
        // Using this model builder,we can seed data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {// In the angular brackets, we specify what entity we want to work on
            base.OnModelCreating(modelBuilder); //Keys of the identity tables are mapped in the on modelcreating. Must hav ethis when using IdentityDbContext
            modelBuilder.Entity<Category>().HasData( // On Categories, we want to ceate these three records
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Sci-Fi", DisplayOrder = 2},
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
                );

            modelBuilder.Entity<Company>().HasData( // On Categories, we want to ceate these three records
               new Company { Id = 1, Name = "Tech Solution", StreetAddress = "123 Tech Street", City="Tech City", PostalCode="12121", State="IL", PhoneNumber="7631463734"},
               new Company { Id = 2, Name = "Vivid Books", StreetAddress = "777 Vid Street", City = "Vid City", PostalCode = "98788", State = "IL", PhoneNumber = "47843178439" },
               new Company { Id = 3, Name = "Readers Club", StreetAddress = "345 Main Street", City = "Lala Land", PostalCode = "34616", State = "NY", PhoneNumber = "87943978434" }
               );

            modelBuilder.Entity<Product>().HasData( // On Categories, we want to ceate these three records
                new Product
                {
                    Id = 1,
                    Title = "Fortune of Time",
                    Author = "Billy Spark",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "SWD9999001",
                    ListPrice = 99,
                    Price = 90,
                    Price50 = 85,
                    Price100 = 80,
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Title = "Dark Skies",
                    Author = "Nancy Hoover",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "CAW777777701",
                    ListPrice = 40,
                    Price = 30,
                    Price50 = 25,
                    Price100 = 20,
                    CategoryId = 2

                },
                new Product
                {
                    Id = 3,
                    Title = "Vanish in the Sunset",
                    Author = "Julian Button",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "RITO5555501",
                    ListPrice = 55,
                    Price = 50,
                    Price50 = 40,
                    Price100 = 35,
                    CategoryId = 1
                },
                new Product
                {
                    Id = 4,
                    Title = "Cotton Candy",
                    Author = "Abby Muscles",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "WS3333333301",
                    ListPrice = 70,
                    Price = 65,
                    Price50 = 60,
                    Price100 = 55,
                    CategoryId = 3
                },
                new Product
                {
                    Id = 5,
                    Title = "Rock in the Ocean",
                    Author = "Ron Parker",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "SOTJ1111111101",
                    ListPrice = 30,
                    Price = 27,
                    Price50 = 25,
                    Price100 = 20,
                    CategoryId = 2
                },
                new Product
                {
                    Id = 6,
                    Title = "Leaves and Wonders",
                    Author = "Laura Phantom",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "FOT000000001",
                    ListPrice = 25,
                    Price = 23,
                    Price50 = 22,
                    Price100 = 20,
                    CategoryId = 3
                }
                );
               
        }
    }
}
