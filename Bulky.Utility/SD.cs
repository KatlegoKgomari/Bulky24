using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Utility
{
    public static class SD
    {// Here we will have all the constants we need for our website

        //Roles (we have these constants instead of having string everywhere)
        public const string Role_Customer = "Customer";
        // If a user is given a role of company, they must belong to some company so we need to manage a list of the companies that are authorised in the system. We need to perform Crud operations on the compnay model
        public const string Role_Company = "Company"; //Company does not need to make payment right away. They have 30days to pay so we will have different functionality for them and they can be registered by an admin user
        public const string Role_Admin = "Admin"; // Can perfom all CRUD operations on content management
        public const string Role_Employee = "Employee"; //Can modify shipping of product and other details

		//Static fields pertaining to order status 
		public const string StatusPending = "Pending";
		public const string StatusApproved = "Approved";
		public const string StatusInProcess = "Processing";
		public const string StatusShipped = "Shipped";
		public const string StatusCancelled= "Cancelled";
		public const string StatusRefunded = "Refunded";

		public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
		public const string PaymentStatusRejected= "Rejected";

        public const string SessionCart = "SessionShoppingCart"; //When we work with session, it is basically a key value


    }
}
