using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public List<Category> CategoryList { get; set; }//Created a property

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet() //This is exactly what we were doing in the controller but now we have to do it on te onGet handler
        { // When we have the onGet and onPost handler, we do not have to write return view
            // The return type is void because whatever we are setting here in the page model is easily retrievable/accessable in the cshtml file
            CategoryList = _db.Categories.ToList();
        }
    }
}
