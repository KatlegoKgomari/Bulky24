using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty] //This will automatically bind the shoppingcartvm. This works exactly like razor pages. When they click submit, the shopping cart model will automaticallly be populated with the values thatwere entered. This means that we do not have to put the shopping cart vm in the paramters of the Sumary post method
        public ShoppingCartVM shoppingCartVM { get; set; }
        private readonly IEmailSender _emailSender; //Must inject this in order to send email
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {//Retrieve shopping cart and pass it to our view but we laso need an IEnumerable of shopping cart as well as the total so we make a viewmodel 
            //We require a shopping cart for a user  and we need user id for that - remember we use cklaims identity
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId= claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //based on the userid, we can populate the viewmodel
            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList(),
                OrderHeader = new()
            };

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = getPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(shoppingCartVM);
        }

        
        public IActionResult Plus(int itemId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == itemId);
            cartFromDb.Count+=1;
            _unitOfWork.ShoppingCartRepository.UpdateShoppingCart(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        
        public IActionResult Minus(int itemId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == itemId, tracked: true);
            if(cartFromDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
                _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count=-1;
                _unitOfWork.ShoppingCartRepository.UpdateShoppingCart(cartFromDb);
            }
            
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        
        public IActionResult Remove(int itemId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == itemId, tracked:true); //Wheen we get the cartfromdb, we are not tracking that entity and when we do that remove, that entity is no loger tracked by ef core . But we wantef core to track this object 
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //based on the userid, we can populate the viewmodel
            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList(),
                OrderHeader = new()
            };

            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u=> u.Id == userId);
            //Once we have applicationUser, we can manually update the values that are in orderheader
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = getPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            
            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")] //Because the action name is not Summary, we need to explicitly say that this is the summary action method. but we also cannot name it Summary because there isalready another method names Summary with the same header
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //based on the userid, we can populate the viewmodel
            shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product").ToList();
				
            shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            //When you try to add somethng to the database it will automatically add the correspoding navigation. Thus, it will think that you ae tring to add a new entity 
            //If you do not want that, then you should never population a navigation property when you are trying to insert a record
            //shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);

            // Do this instead:
            ApplicationUser applicationUser = _unitOfWork.ApplicationUserRepository.GetFirstOrDefault(u => u.Id == userId);

			foreach (var cart in shoppingCartVM.ShoppingCartList)
			{
				cart.Price = getPriceBasedOnQuantity(cart);
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

            //Checking if the user belongs to a company
            if(applicationUser.CompanyId.GetValueOrDefault() == 0) //using the getfirstordefault method because companyid could be null and this method handles that
            { //regulr customer account 
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            { //it is a company account
				shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}

			// Now we can put the orderheader in the database
			_unitOfWork.OrderHeaderRepository.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            //Creating orderDetail 
            foreach(var cart in shoppingCartVM.ShoppingCartList)
            {
                // Have to create orderdetail for each item in the shoppingCartList
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                //Adding it to the database
                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }

			if (applicationUser.CompanyId.GetValueOrDefault() == 0) 
			{ //if it's a reular customer, we need to get payment)
              //Stripe logic
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    // When we go to this page, how will we know that the payment has been successful or not? We will need to get the session agan from stripe and check its status 
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",// If it is successful, this is where it should redirect
                    CancelUrl = domain + "customer/cart/index",
					LineItems = new List<SessionLineItemOptions>(), //Will have all the product details. This is a built-in class in the stripe package
					Mode = "payment",
				};

                foreach(var item in shoppingCartVM.ShoppingCartList)
				{
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        //Price data is basically data used to create a new price object
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),  //configuring the line items here and to get all the items, we iterate through the shopping cart list
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title //We could even have the image of the item her if we wanted
                            }
                        },
                        Quantity = item.Count //Based on the quantity, stripe will determine the final price
                     };
                    options.LineItems.Add(sessionLineItem);
				}
				var service = new SessionService(); //Creating a new sessionservice. After this, we will redirect to the sripe website
				Session session= service.Create(options); //on service, we are creating it with all the options. We use a session variable because we expect a reply from the server. Inside this variable, we have the sesssion id and the payment intent id and we update them in our orderheader tabke before we redirect back

                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId); // PaymentIntentID is null right now. In a session object, it is only populaed once it is successful
                _unitOfWork.Save();
				Response.Headers.Add("Location", session.Url); // teh url that we have to redirect is inside the session object
                return new StatusCodeResult(303); //Redirecting to the url provided in the line before
			}
			//Redirecting to the confirmation page
			return RedirectToAction(nameof(OrderConfirmation), new {id= shoppingCartVM.OrderHeader.Id}); //also passing the id
		}

        public IActionResult OrderConfirmation(int id)
        {
            // Based on the id, we retrieve the complete order header
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u=> u.Id == id,includeProperties:"ApplicationUser");
            //If the payment status is delayed payment, then we do not care about it right now because then it is a company that we are dealing with
            if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                // This is an order by an ordinary customer. So we need to retrieve a stripe session
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);//retrieving a session object. We get the session id which we saved in order header

                if(session.PaymentStatus.ToLower() == "paid") //This means that actual payment went through and in that case, this session will have the paymentIntent id
                {
                    //so we update the payment intent id
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    //We also have to update the order status
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear(); // clearing the session after we have paid. We can do it just like this because we know we have justone session but typically, we clear on the key name
            }
            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", $"<p>New Order Created - {orderHeader.Id}</p>");
            //Remove the shoppingcart and make it empty 
            List<ShoppingCart> shoppingCarts= _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            //Removing from database
            _unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }
		// We need a helper method for the price that will calculate which price should be used based on the quantity
		private double getPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price; //default price
            }
            else if( shoppingCart.Count>50 && shoppingCart.Count <= 100){
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }

        
    }
}
