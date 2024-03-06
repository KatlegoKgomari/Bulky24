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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
       

        public CompanyController(IUnitOfWork unitOfWork) //Here you are asking Dependency injection to provide an implemention 
        {
            _unitOfWork = unitOfWork;
            
        }
        // It has one action method. How will this endpoint be triggered?
        public IActionResult Index()
        {//We do not have to write any sql, entity framework core will handle the retrieving of the data 
            // we can access all the db sets that we added. 
            List<Company> objCompanyList = _unitOfWork.CompanyRepository.GetAll().ToList();
           
            return View(objCompanyList); // When the actiob method is called, it returns back a view
            // We can only pass one object in a view
        }

        public IActionResult Upsert(int? id) //Upsert stands for update or insert. You may or may not have an id. if you are creating == no id. Updating == id
        {
            if(id == null || id == 0)
            {
                return View(new Company()); //We send it to the view and it's got empty fields
                
            }
            else
            {
                //If we have a value for id, then it is an edit
                Company companyObj = _unitOfWork.CompanyRepository.GetFirstOrDefault(u => u.Id == id);
                return View(companyObj);
            }
            
        }

        //We get a category object because that is the modelwe defined in the view
        [HttpPost]
        public IActionResult Upsert(Company company) //When a file s uploaded, we get it in the iform file and we have to capture this file and save inside the images/product path 
        {
          
            if (ModelState.IsValid)// This will basically check if the model state(the category object) that we have is valid. This means that we will go to the category model and examine all the validations. If these aren't met, then the model state is not valid and it will not go to the database and we will not save category
            {
                
                // We can tell whther we are adding or updating based on wether the id is available
                if(company.Id == 0)
                {
                    // then we are adding
                    _unitOfWork.CompanyRepository.Add(company); //This add method is provided by entity framework core.This is the only line that you need to add a category (no need for insert statements). the add statement onl tells it that we want to add. nothing else. It is the save cahnges method that will execute all the changes
                }
                else
                {
                    _unitOfWork.CompanyRepository.UpdateCompany(company);
                }
                _unitOfWork.Save(); // this line of code actually executes what needs to be done. Previous lines of code kept track of what changes needed to be done. This works well because you might want to make 5 changes at one time and you odn't want to go back and forth for each one
                TempData["success"] = "Product created successfully"; // And based on that key name, we can access its value.
                return RedirectToAction("Index"); //Once the category is added, we want to redict them to the list of categories. We can't go back to teh view, but we can go back to teh index action.There it will reload the categories because when a category is added, we have to reload and pass that to the view
                                                  //Rather than redirecting to a view, we also have something called Redirect to action. If you are in the same controller, you just have to write the action name but uf you want to go to another controller, you need to specify it's name as teh second parameter. 
                                                  //If everything is valid, create and return back to the index
            }
            else
            {
                return View(company);
                
            }


        }

        

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() // We can say get all because we are using mvc architecture
        {
            List<Company> objCompanyList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new {data = objCompanyList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id) // We can say get all because we are using mvc architecture
        {
            var companyToBeDeleted = _unitOfWork.CompanyRepository.GetFirstOrDefault(u=> u.Id == id);
            if(companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.CompanyRepository.Remove(companyToBeDeleted);
            _unitOfWork.Save(); //Don't forget the save
            return Json(new { success = true, message = "Delete Successful" }); //Our API is being invoked and it returns Json
        }
        #endregion

    }
}
