using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    //[BindProperties] for when we have multiple properties
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        [BindProperty] //Makes sure to bind p roperty so when we post it, it will automatically be binded and available in the post handler
        public Category Category { get; set; } //since we will be creating a category

        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet()
        {
        }

        // adding a post endpoint here 
        public IActionResult OnPost() //IActionResult and not void because we want to return a page
        {
            _db.Categories.Add(Category); 
            _db.SaveChanges();
            TempData["success"] = "Category created successfully";
            return RedirectToPage("Index");
        }
    }
}
