using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Security.Cryptography.Xml;
using static NuGet.Packaging.PackagingConstants;


namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] //only these roles can access the action methods
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;


        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager) //Here you are asking Dependency injection to provide an implemention 
        {
            _db = db;
            _userManager = userManager;
            
        }
        // It has one action method. How will this endpoint be triggered?
        public IActionResult Index()
        {//We do not do anything here, we  just return view because we will be using datatable
            return View();
        }


        public IActionResult RoleManagement(string userId)
        {
            string RoleID = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;
            RoleManagementVM RoleVM = new RoleManagementVM()
            {
                ApplicationUser = _db.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId),
                //Populate the drop downs
                RoleList = _db.Roles.Select(i => new SelectListItem {
                    Text = i.Name,
                    Value = i.Name
                }),

                CompanyList = _db.Companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            RoleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == RoleID).Name;
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            //Because we did not bind the property, we have to do this 
            string RoleID = _db.UserRoles.FirstOrDefault(u=> u.UserId == roleManagementVM.ApplicationUser.Id).RoleId; // Current role 
            //retrieving old role
            string oldRole = _db.Roles.FirstOrDefault(u => u.Id == RoleID).Name;
            if(!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                // A role was updated 
                ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagementVM.ApplicationUser.Id);
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Company) // If they are a company, then along with assigning role, we need to assign company
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if(oldRole== SD.Role_Company)
                {
                    //If old role was a company role 
                    applicationUser.CompanyId = null;
                }
                _db.SaveChanges();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            
            
            return View("Index");
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() // We can say get all because we are using mvc architecture
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u=> u.Company).ToList();
            var userRoles = _db.UserRoles.ToList(); // This is the mapping that had application user id and role id
            var roles = _db.Roles.ToList(); 

            foreach(var user in objUserList)
            {
                //We  also want the role of the user. We get their role id
                //based on the id, we can find teh role id and thus the role of the user
                var roleId = userRoles.FirstOrDefault(u=> u.UserId == user.Id).RoleId;
                user.Role= roles.FirstOrDefault(u=> u.Id == roleId).Name;
                if (user.Company == null)
                {
                    user.Company = new() { Name = "" }; //If they are not a  company, we make the company name an empty string  so an error does not come up in user.js
                }
            }

            return Json(new {data = objUserList});
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id) // We can say get all because we are using mvc architecture
        {
            var  objFromDb = _db.ApplicationUsers.FirstOrDefault(u=>u.Id == id);
            if(objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }
            if (objFromDb.LockoutEnd !=null && objFromDb.LockoutEnd > DateTime.Now)
            { // If date is empty or date is a date in the future, that means that the user is locked
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd= DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges(); //Thsi works because the record here is being tracked by ef core
            return Json(new { success = true, message = "Operation Successful" }); //Our API is being invoked and it returns Json
        }
        #endregion

    }
}
