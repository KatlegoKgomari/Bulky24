using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("admin")]
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        //Will use datatable to display all the orders 
        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
            //Based on the orderId, we can populate the orderVM
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetailRepository.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product") //bause there will be more than one order detail 
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles= SD.Role_Admin + "," + SD.Role_Employee)] //Can give the roles as a comma separated list
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            //Manually map all of the fields we want to update here
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeaderRepository.UpdateOrderHeader(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Upadated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Upadated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder() //must also update tracking and carrier information
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id);
            //Updatin all the relevant properties and then putting it back
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            //If the payment status is delaye, then we have to update the payment due date as well
            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime( DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeaderRepository.UpdateOrderHeader(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder() //must also update tracking and carrier information
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)//This means that payment was already done and we need to give them a refund
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntendId,
                };
                var service = new RefundService();
                Refund refund = service.Create(options); //Refund has been processed 
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                //We do not refund them we just cancel
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.OrderHeaderRepository.UpdateOrderHeader(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [ActionName(nameof(Details))]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u=>u.Id == OrderVM.OrderHeader.Id, includeProperties:"ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetailRepository.GetAll(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "Product");

            //Stripe logic
            var domain = "https://localhost:7075/";
            var options = new SessionCreateOptions
            {
                // When we go to this page, how will we know that the payment has been successful or not? We will need to get the session agan from stripe and check its status 
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",// If it is successful, this is where it should redirect
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(), //Will have all the product details. This is a built-in class in the stripe package
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
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
            Session session = service.Create(options); //on service, we are creating it with all the options. We use a session variable because we expect a reply from the server. Inside this variable, we have the sesssion id and the payment intent id and we update them in our orderheader tabke before we redirect back

            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId); // PaymentIntentID is null right now. In a session object, it is only populaed once it is successful
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url); // teh url that we have to redirect is inside the session object
            return new StatusCodeResult(303); //Redirecting to the url provided in the line before
        
           
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            // Based on the id, we retrieve the complete order header
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(u => u.Id == orderHeaderId);
            //If the payment status is delayed payment, then we do not care about it right now because then it is a company that we are dealing with
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment) //Wantto make sure that payment went through
            {
                // This is an order by an ordinary customer. So we need to retrieve a stripe session
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);//retrieving a session object. We get the session id which we saved in order header

                if (session.PaymentStatus.ToLower() == "paid") //This means that actual payment went through and in that case, this session will have the paymentIntent id
                {
                    //so we update the payment intent id
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    //We also have to update the order status
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            
            return View(orderHeaderId);
        }


        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status) // We can say get all because we are using mvc architecture
		{
            IEnumerable<OrderHeader> objOrderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList(); // Including applicaton user because that is teh foreign entity that we want to fill
            }
            else
            {
                //Filtering based on user id because we want customers to see only their orders
                //get userid using claimsIdentity
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(u=>u.ApplicationUserId == userId, includeProperties:"ApplicationUser");
            }
            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    //For all, we do not have to do anything
                    break;
            }
            return Json(new { data = objOrderHeaders });
		}

		
		#endregion
	}
}
