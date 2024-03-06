using Bulky.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bulky.Models;
using System.Linq.Expressions;
using Bulky.DataAccess.Data;

namespace Bulky.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        // all the other methods have been remmoved because we have an implementation for them. This is specified in Repository and here we specify that it will be a category
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db) //When we get this iplementation, we want to get it to the base class (so whatever db we get here,we pass to the repository)
        {
            _db = db;
        }
        

        public void UpdateOrderHeader(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

		void IOrderHeaderRepository.UpdateStatus(int id, string orderStatus, string? paymentStatus)
		{
	        var orderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
		}

		void IOrderHeaderRepository.UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
		{
			var orderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if (!string.IsNullOrEmpty(sessionId))
			{
				orderFromDb.SessionId = sessionId;
			}
            //a session ID gets generated when a user tries to make a payment. When it is successful, then a payment intent ID gets generated.
            if (!string.IsNullOrEmpty(paymentIntentId)) //then the paymeant was successful
            {
                orderFromDb.PaymentIntendId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
		}
	}
}
