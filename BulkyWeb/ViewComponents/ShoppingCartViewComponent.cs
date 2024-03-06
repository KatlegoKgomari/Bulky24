using Bulky.DataAccess.Repository.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.ViewComponents
{
    public class ShoppingCartViewComponent:ViewComponent
    {
        //This will be the backend file for our view component
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //Method that will handle the backedn functionality for our shopping cart view component
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //retrieving the userid of the logged in user 
            var claimsIdentity = (ClaimsIdentity)User.Identity;//Getting the user id for the logged in user. 
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); //Vlaue could be null so we don't have .value at the end

            if (claim != null)
                
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) == null) //If the sessio is already loaded, then wedo not have to go all the way to th database,we can just retun the value
                {//This means that the user is logged in 
                    HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == claim.Value).Count()); //Userid will be inside claim. value 
                   
                    
                }
                
                return View(HttpContext.Session.GetInt32(SD.SessionCart)); // So the model that we are returning to the view is an integer
            }
            else
            {
                HttpContext.Session.Clear(); //So we no longer need it in the logout
                return View(0);
            }
        }
    }
}
