using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]  
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }

        // In our store, if you buy books in bulk, you get a discount 
        // So based onthe different quantities, we will have different prices 

        [Required]
        [Display(Name = "List Price")]
        [Range(1,1000)] //Price range
        public double ListPrice { get; set; }

        [Required]
        [Display(Name = "Price for 1-50")]
        [Range(1, 1000)] //Price range
        public double Price { get; set; }

        [Required]
        [Display(Name = "Price for 50+")]
        [Range(1, 1000)] //Price range
        public double Price50 { get; set; }

        [Required]
        [Display(Name = "Price for 100+")] // If they buy more than 100 books 
        [Range(1, 1000)] //Price range
        public double Price100 { get; set; }

        public int CategoryId { get; set; } // If we add this column on its own, table will not know that this is a foreign key to the category table. In order to explicitly define that, we need a navigation property to the Category table
        [ForeignKey("CategoryId")] //We specify that this category p is used for foreign key navigation for the Category ID
        [ValidateNever]
        public Category Category { get; set; } // Said navigation property to category table
        [ValidateNever]
        public string ImageUrl { get; set; } //We want this to be a file upload
    }

}// Before we add this  product table to our database, we have to applicationdbContext
