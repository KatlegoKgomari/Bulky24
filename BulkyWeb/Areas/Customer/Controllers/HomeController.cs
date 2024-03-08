using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")] //We are telling the controller which area it belongs to
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            

            IEnumerable<Product> productList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category,ProductImages");
            return View(productList);  //returning the product list back to the view
        }

        public IActionResult Details(int id)
        {
            ShoppingCart cart = new ShoppingCart
            {
                Product = _unitOfWork.ProductRepository.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,ProductImages"),
                Count = 1, //default count
                ProductId = id

            };
            
            return View(cart);  //returning the product list back to the view
        }
        [HttpPost]
        [Authorize] //If someone is posting, they must be an authorized user (though we are ot concened about what role they are in)
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;//Getting the user id for the logged in user. We have to get theh claims identity which is provided inside the user object which in provided by default. We hav etoexplicitly cast it to claims identity. There is a special claim called identifier that willhave the user id for the logged in user
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            //we need to make sure that cart does not already exist for that user and productId
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.ApplicationUserId== userId && u.ProductId==shoppingCart.ProductId) ;
            if(cartFromDb != null)
            {
                // Then shopping cart already exists
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCartRepository.UpdateShoppingCart(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                // Add cart record
                _unitOfWork.ShoppingCartRepository.Add(shoppingCart);
                _unitOfWork.Save();
                //Whenever we add a new item to the cart, we will be adding that item to session (We are seeting a session of integer). When working with .net, you have setInt and setstring and they are avalable by default
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId).Count()); //When we are setting this, we need a key and a value. For value, we  have to go to the database and get all of the shoppingcarts for that user id. Count is the number of items that the logged in user has
                //We want to display the count in our session on the _layout page.
                //It is important to not confuse the count in shopping cart with the count method for collections
            }
            TempData["success"] = "Cart Updated Successfully";
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
