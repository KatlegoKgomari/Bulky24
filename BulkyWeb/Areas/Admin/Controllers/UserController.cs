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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserController( UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork) //Here you are asking Dependency injection to provide an implemention 
        {
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            
        }
        // It has one action method. How will this endpoint be triggered?
        public IActionResult Index()
        {//We do not do anything here, we  just return view because we will be using datatable
            return View();
        }


        public IActionResult RoleManagement(string userId)
        {
            //string RoleID = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;
            RoleManagementVM RoleVM = new RoleManagementVM()
            {
                ApplicationUser = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId, includeProperties:"Company"),
                //Populate the drop downs
                RoleList = _roleManager.Roles.Select(i => new SelectListItem {
                    Text = i.Name,
                    Value = i.Name
                }),

                CompanyList = _unitOfWork.CompanyRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u=> u.Id == userId)).GetAwaiter().GetResult().FirstOrDefault();
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            //Because we did not bind the property, we have to do this 
            //string RoleID = _db.UserRoles.FirstOrDefault(u=> u.UserId == roleManagementVM.ApplicationUser.Id).RoleId; // Current role 
            //retrieving old role
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == roleManagementVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();
            ApplicationUser applicationUser = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == roleManagementVM.ApplicationUser.Id);
            if (!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                // A role was updated 
                
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Company) // If they are a company, then along with assigning role, we need to assign company
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if(oldRole== SD.Role_Company)
                {
                    //If old role was a company role 
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUserRepository.Update(applicationUser);
                _unitOfWork.Save();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldRole==SD.Role_Company && applicationUser.CompanyId!= roleManagementVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUserRepository.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }
            
            return View("Index");
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() // We can say get all because we are using mvc architecture
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUserRepository.GetAll(includeProperties:"Company").ToList();
            
            foreach(var user in objUserList)
            {
                //We  also want the role of the user. We get their role id
                //based on the id, we can find teh role id and thus the role of the user
             
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
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
            var  objFromDb =_unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u=> u.Id==id);
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

            _unitOfWork.ApplicationUserRepository.Update(objFromDb);
            _unitOfWork.Save();

            //_db.SaveChanges(); //Thsi works because the record here is being tracked by ef core
            return Json(new { success = true, message = "Operation Successful" }); //Our API is being invoked and it returns Json
        }
        #endregion

    }
}
