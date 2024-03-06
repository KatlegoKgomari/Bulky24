using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bulky.DataAccess.Repository.IRepository
{ //We need our category to implement the repository
    public interface IOrderHeaderRepository: IRepository<OrderHeader> // We know the class on which we want the implementatation for this class. when we need the implementation for Icategoryrepository, we to get the base functionality from repository.
    // that will make sure we have all of these base methods where entity will be category.
    // On top of that, we needed two more functionalities and those are update and save methods.
    {
        void UpdateOrderHeader(OrderHeader obj);

        //What if, based on the id, we only want to update the order status or paymnt status
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null); //PaymentStatus can be null because we will be updating orderstatus a lot but paymant status is set to approved,it stays at approved (same thing for approved for delayed payment)
        //Based on the order header id, we can also update session id and paymentitentid
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);


        
    }
}
