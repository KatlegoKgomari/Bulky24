using Bulky.Models;
using Bulky.DataAccess.Data;


using Microsoft.AspNetCore.Mvc;
using System.IO;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Bulky.Utility;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]// We are telling the controller that itbelongs to a specific area
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        //we were using application DB context but now we do not have to use this directly. We can access the category repository.

        private readonly IUnitOfWork _unitOfWork;


        public CategoryController(IUnitOfWork unitOfWork) //Here you are asking Dependency injection to provide an implemention 
        {
            _unitOfWork = unitOfWork;
        }
        // It has one action method. How will this endpoint be triggered?
        public IActionResult Index()
        {//We do not have to write any sql, entity framework core will handle the retrieving of the data 
            // we can access all the db sets that we added. 
            List<Category> objCategoryList = _unitOfWork.CategoryRepository.GetAll().ToList();
            return View(objCategoryList); // When the actiob method is called, it returns back a view
        }

        public IActionResult AddNewCategory()
        {
            return View();
        }

        //We get a category object because that is the modelwe defined in the view
        [HttpPost]
        public IActionResult AddNewCategory(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                // This is a custom validation, we want to make sure that name and displayorder are not the same
                ModelState.AddModelError("Name", "The Display Order cannot match the name."); //On the model state that we have, we want to add a custom error message. We need to write a key and teh message. the key is the field that we are assigning it to. This fiels is found in the category view in asp-for. It also works hand-in-hand with asp-validation-for (the error message is displayed where the error message would typically be displayed)
            }
            if (obj.Name != null && obj.Name.ToLower() == "test") //This is laso custom validation
            {
                // We want to say that test is an invalid name. But we do not want this error essage to be associated with any particular key (Thus it will not appear under the textbox of that key but just at the top-especially because we have said "all" in asp-validation-summary)
                ModelState.AddModelError("", obj.Name.ToLower() + " is not a valid name.");
            }
            if (ModelState.IsValid)// This will basically check if the model state(the category object) that we have is valid. This means that we will go to the category model and examine all the validations. If these aren't met, then the model state is not valid and it will not go to the database and we will not save category
            {
                _unitOfWork.CategoryRepository.Add(obj); //This add method is provided by entity framework core.This is the only line that you need to add a category (no need for insert statements). the add statement onl tells it that we want to add. nothing else. It is the save cahnges method that will execute all the changes
                _unitOfWork.Save(); // this line of code actually executes what needs to be done. Previous lines of code kept track of what changes needed to be done. This works well because you might want to make 5 changes at one time and you odn't want to go back and forth for each one
                TempData["success"] = "Category created successfully"; // And based on that key name, we can access its value.
                return RedirectToAction("Index"); //Once the category is added, we want to redict them to the list of categories. We can't go back to teh view, but we can go back to teh index action.There it will reload the categories because when a category is added, we have to reload and pass that to the view
                                                  //Rather than redirecting to a view, we also have something called Redirect to action. If you are in the same controller, you just have to write the action name but uf you want to go to another controller, you need to specify it's name as teh second parameter. 
                                                  //If everything is valid, create and return back to the index
            }

            //Else, we want to return back to the view itself
            return View();

        }

        // This shows us the view. This is also the get action method. Since there is no http thing above it, it is a get metjod by default
        public ActionResult Edit(int? id) // We need the id of what the user wants to edit. The question mark makes it nullable
        {
            //a validation 
            if (id == null || id == 0)
            {
                return NotFound(); //You can have an error page an return back to that view which will be an error view displaying that something is not valid 
            }

            //If the id is valid, we need to retrieve that category from the database. Entity framework core has method for that
            Category? categoryFromDb = _unitOfWork.CategoryRepository.GetFirstOrDefault(u => u.Id == id);//find will work on the primary key of that model therefore, we can pass in id. Find only works on the primary key
            //Category? categoryFromDb1= Db.Categories.FirstOrDefault(u => u.Id == id); //it will use a link operation. u.id is equal to the id that is being passed. First it will check if there is any record,if not, it will return a null object. This method will work even if the key is not primary so you coul even search on name or even say Name.contains
            //Category? categoryFromDb2 = Db.Categories.Where(u => u.Id == id).FirstOrDefault(); //we make them nullable and the warnings go away. First or Defaut works well but this one also helps when you additionl filterig to do

            // since categoryFromDb could be null (if teh category is not found), we do a check
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            //But if it is found, we pass it to our view nand display it
            return View(categoryFromDb);  //and our cshtml file has the model at the top so it knows to put a name 
        }

        [HttpPost]
        public IActionResult Edit(Category category) //Retrieve an object an update the category
        {
            // We do client side validation
            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepository.UpdateCategory(category); // Entity framework core has a method for updating. Based on the id that is populated in the object, it will automatically update all the other properties that are there for that category in the database
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int id)//same thing we did in delete action method, find object and display it
        {
            //a validation 
            if (id == null || id == 0)
            {
                return NotFound(); //You can have an error page an return back to that view which will be an error view displaying that something is not valid 
            }

            //If the id is valid, we need to retrieve that category from the database. Entity framework core has method for that
            Category? categoryFromDb = _unitOfWork.CategoryRepository.GetFirstOrDefault(u => u.Id == id); //find will work on the primary key of that model therefore, we can pass in id. Find only works on the primary key
                                                                                                          // Category? categoryFromDb1 = Db.Categories.FirstOrDefault(u => u.Id == id); //it will use a link operation. u.id is equal to the id that is being passed. First it will check if there is any record,if not, it will return a null object. This method will work even if the key is not primary so you coul even search on name or even say Name.contains
                                                                                                          // Category? categoryFromDb2 = Db.Categories.Where(u => u.Id == id).FirstOrDefault(); //we make them nullable and the warnings go away. First or Defaut works well but this one also helps when you additionl filterig to do

            // since categoryFromDb could be null (if teh category is not found), we do a check
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            //But if it is found, we pass it to our view nand display it
            return View(categoryFromDb);  //and our cshtml file has the model at the top so it knows to put a name 
        }

        [HttpPost, ActionName("Delete")] // We have to tell it explicitly that the name for this endpoint is delete because in the form, when we are posting, it will look for the same delet action method. We use actionName to do that 
        public IActionResult DeletePost(int id) //When deleting, you could get the whole category object or just the id. We can'r use the same name for the action methods because the parameters are also the same
        {
            Category? obj = _unitOfWork.CategoryRepository.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.CategoryRepository.Remove(obj); //It expects a category method
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
