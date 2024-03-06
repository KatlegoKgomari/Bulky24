using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;

         //Makes sure to bind p roperty so when we post it, it will automatically be binded and available in the post handler
        public Category? Category { get; set; } //since we will be creating a category

        public EditModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet(int? id) // we need to retrieve the category that the user wants to edit.  Made id nullable
        {
            if((id != null) && (id !=0)) 
            { //then we can populate the category
                Category = _db.Categories.Find(id);
            }

            


        }

        // adding a post endpoint here 
        public IActionResult OnPost() //IActionResult and not void because we want to return a page
        {
            if(ModelState.IsValid)
            {
                _db.Categories.Update(Category);
                _db.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToPage("Index");
            }
            return Page();
        }

    }
}
