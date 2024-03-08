using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; } // In here we will have all the repositories so ICategoryRepository
        IProductRepository ProductRepository { get; }
        ICompanyRepository CompanyRepository { get; }
        IApplicationUserRepository ApplicationUserRepository { get; }

        IShoppingCartRepository ShoppingCartRepository { get; } 
        IOrderHeaderRepository OrderHeaderRepository { get; }
        IOrderDetailRepository OrderDetailRepository { get; }
        IProductImageRepository ProductImageRepository { get; }
        void Save();
        
    }
}
