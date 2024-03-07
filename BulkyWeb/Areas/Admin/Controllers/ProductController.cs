using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Immutable;
using System.Security.Cryptography.Xml;
using static NuGet.Packaging.PackagingConstants;


namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] //only these roles can access the action methods
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;   // We inject it using dependency injection. And we will b able to access the wwwroot folder

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) //Here you are asking Dependency injection to provide an implemention 
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        // It has one action method. How will this endpoint be triggered?
        public IActionResult Index()
        {//We do not have to write any sql, entity framework core will handle the retrieving of the data 
            // we can access all the db sets that we added. 
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category").ToList();
           
            return View(objProductList); // When the actiob method is called, it returns back a view
            // We can only pass one object in a view
        }

        public IActionResult Upsert(int? id) //Upsert stands for update or insert. You may or may not have an id. if you are creating == no id. Updating == id
        {
              
            // ViewBag.CategoryList = CategoryList;
           // ViewData["CategoryList"] = CategoryList; //Like a dictionary key-value pair
            ProductVM productVM = new()  //This looks good for create but if you want to edit,y you need to retrieve the product and assign it to the product inside of th viewmodel
            {
                CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name, //We do this because we want the information to be of type SelectListItem. This is not the same as casting as you get to specifyteh vakues that you keep here. This is called a Projection
                    Value = u.Id.ToString(),
                }), 
                Product = new Product()

            };
            if(id == null || id == 0)
            {
                return View(productVM); //This means that it's create and we can get back directly to the view
                
            }
            else
            {
                //If we have a value for id, then it is an edit
                productVM.Product = _unitOfWork.ProductRepository.GetFirstOrDefault(u => u.Id == id, includeProperties:"Category");
                return View(productVM);
            }
            
        }

        //We get a category object because that is the modelwe defined in the view
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file) //When a file s uploaded, we get it in the iform file and we have to capture this file and save inside the images/product path 
        {
          
            if (ModelState.IsValid)// This will basically check if the model state(the category object) that we have is valid. This means that we will go to the category model and examine all the validations. If these aren't met, then the model state is not valid and it will not go to the database and we will not save category
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath; // That path will basically give us the root folder which is www root folder.

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); //Gives us a random name for our file
                    string productPath = Path.Combine(wwwRootPath, @"images/product"); // location to save file

                    //if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    //{
                    //    //If the user is uploading a new image and there is already an existing image for that book. We want to delete the old image
                    //    var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                    //    if(System.IO.File.Exists(oldImagePath))
                    //    {
                    //        System.IO.File.Delete(oldImagePath);
                    //    }
                    //}
                    //// Then we upload a new image and e update the url
                    //using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))//it's create because we are creating a new file
                    //{
                    //    file.CopyTo(fileStream); //W want to copy the fle to teh file stream
                    //}
                    //productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                // We can tell whther we are adding or updating based on wether the id is available
                if(productVM.Product.Id == 0)
                {
                    // then we are adding
                    _unitOfWork.ProductRepository.Add(productVM.Product); //This add method is provided by entity framework core.This is the only line that you need to add a category (no need for insert statements). the add statement onl tells it that we want to add. nothing else. It is the save cahnges method that will execute all the changes
                }
                else
                {
                    _unitOfWork.ProductRepository.UpdateProduct(productVM.Product);
                }
                _unitOfWork.Save(); // this line of code actually executes what needs to be done. Previous lines of code kept track of what changes needed to be done. This works well because you might want to make 5 changes at one time and you odn't want to go back and forth for each one
                TempData["success"] = "Product created successfully"; // And based on that key name, we can access its value.
                return RedirectToAction("Index"); //Once the category is added, we want to redict them to the list of categories. We can't go back to teh view, but we can go back to teh index action.There it will reload the categories because when a category is added, we have to reload and pass that to the view
                                                  //Rather than redirecting to a view, we also have something called Redirect to action. If you are in the same controller, you just have to write the action name but uf you want to go to another controller, you need to specify it's name as teh second parameter. 
                                                  //If everything is valid, create and return back to the index
            }
            else
            {
                productVM.CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem //When you return back, you have to populate the dropdown again
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
                return View(productVM); // This makes sure that if the model state fails, we still repopulate the dropdown
                
            }

           
            

        }

        // This shows us the view. This is also the get action method. Since there is no http thing above it, it is a get metjod by default
        //public ActionResult Edit(int? id) // We need the id of what the user wants to edit. The question mark makes it nullable
        //{
        //    //a validation 
        //    if (id == null || id == 0)
        //    {
        //        return NotFound(); //You can have an error page an return back to that view which will be an error view displaying that something is not valid 
        //    }

        //    //If the id is valid, we need to retrieve that category from the database. Entity framework core has method for that
        //    Product? productFromDb = _unitOfWork.ProductRepository.GetFirstOrDefault(u => u.Id == id);//find will work on the primary key of that model therefore, we can pass in id. Find only works on the primary key
        //    //Category? categoryFromDb1= Db.Categories.FirstOrDefault(u => u.Id == id); //it will use a link operation. u.id is equal to the id that is being passed. First it will check if there is any record,if not, it will return a null object. This method will work even if the key is not primary so you coul even search on name or even say Name.contains
        //    //Category? categoryFromDb2 = Db.Categories.Where(u => u.Id == id).FirstOrDefault(); //we make them nullable and the warnings go away. First or Defaut works well but this one also helps when you additionl filterig to do

        //    // since categoryFromDb could be null (if teh category is not found), we do a check
        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    //But if it is found, we pass it to our view nand display it
        //    return View(productFromDb);  //and our cshtml file has the model at the top so it knows to put a name 
        //}

        //[HttpPost]
        //public IActionResult Edit(Product product) //Retrieve an object an update the category
        //{
        //    // We do client side validation
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.ProductRepository.UpdateProduct(product); // Entity framework core has a method for updating. Based on the id that is populated in the object, it will automatically update all the other properties that are there for that category in the database
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product updated successfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        //public IActionResult Delete(int id)//same thing we did in delete action method, find object and display it
        //{
        //    //a validation 
        //    if (id == null || id == 0)
        //    {
        //        return NotFound(); //You can have an error page an return back to that view which will be an error view displaying that something is not valid 
        //    }

        //    //If the id is valid, we need to retrieve that category from the database. Entity framework core has method for that
        //    Product? ProductFromDb = _unitOfWork.ProductRepository.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category"); //find will work on the primary key of that model therefore, we can pass in id. Find only works on the primary key
        //                                                                                                  // Category? categoryFromDb1 = Db.Categories.FirstOrDefault(u => u.Id == id); //it will use a link operation. u.id is equal to the id that is being passed. First it will check if there is any record,if not, it will return a null object. This method will work even if the key is not primary so you coul even search on name or even say Name.contains
        //                                                                                                  // Category? categoryFromDb2 = Db.Categories.Where(u => u.Id == id).FirstOrDefault(); //we make them nullable and the warnings go away. First or Defaut works well but this one also helps when you additionl filterig to do

        //    // since categoryFromDb could be null (if teh category is not found), we do a check
        //    if (ProductFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    //But if it is found, we pass it to our view nand display it
        //    return View(ProductFromDb);  //and our cshtml file has the model at the top so it knows to put a name 
        //}

        //[HttpPost, ActionName("Delete")] // We have to tell it explicitly that the name for this endpoint is delete because in the form, when we are posting, it will look for the same delet action method. We use actionName to do that 
        //public IActionResult DeletePost(int id) //When deleting, you could get the whole category object or just the id. We can'r use the same name for the action methods because the parameters are also the same
        //{
        //    Product? obj = _unitOfWork.ProductRepository.GetFirstOrDefault(u => u.Id == id, includeProperties:"Category");
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }

        //    _unitOfWork.ProductRepository.Remove(obj); //It expects a category method
        //    _unitOfWork.Save();
        //    TempData["success"] = "Category deleted successfully";
        //    return RedirectToAction("Index");
        //}

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() // We can say get all because we are using mvc architecture
        {
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category").ToList();
            return Json(new {data = objProductList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id) // We can say get all because we are using mvc architecture
        {
            var productToBeDeleted = _unitOfWork.ProductRepository.GetFirstOrDefault(u=> u.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            //If the product is not null, before we delete it, we need to delete its image as well
            //var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            //if (System.IO.File.Exists(oldImagePath))
            //{
            //    System.IO.File.Delete(oldImagePath);
            //}

            _unitOfWork.ProductRepository.Remove(productToBeDeleted);
            _unitOfWork.Save(); //Don't forget the save
            return Json(new { success = true, message = "Delete Successful" }); //Our API is being invoked and it returns Json
        }
        #endregion

    }
}
