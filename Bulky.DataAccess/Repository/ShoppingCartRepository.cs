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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        // all the other methods have been remmoved because we have an implementation for them. This is specified in Repository and here we specify that it will be a category
        private readonly ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db):base(db) //When we get this iplementation, we want to get it to the base class (so whatever db we get here,we pass to the repository)
        {
            _db = db;
        }
        

        public void UpdateShoppingCart(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }
    }
}
