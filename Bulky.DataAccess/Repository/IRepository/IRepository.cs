using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class // making it generic because we will have many different classes in the future
    {
        // The basic methods that will be needed
        // T will be category or any other generic model on which we want to perform the CRUD operation, or rather, we want to interact with the DB context.

        // T - Category 
        // We need to retrieve all the categories and display all of them 
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter =null, string? includeProperties = null);   // So we can say the return type will be IEnumerable of T, and we can call that method as GetAll. (These are abstract methods. IEnumrable is the return type and getall is the method)


        //If we have to retieve only one category
        // We will be getting a functionand the result will be a  boolean. We can call it filter when we are fetcing an individual record
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false); //The return type will be a single category. We will not be using the plain find method of EF core because it only takes in ID. but if you want some other condition to get one record, you can pass that using the LINQ operator.
        
        // We also need to be aable to create or remove a category. You can also remove multiple categories in a single call
        void Add(T entity); // we have to pass the object that needs to be added.
       
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity); //The parameter here we will receive an IEnumerable or a collection of entities.

        // remember when we were working with EF Core and we were updating or adding anything, we always had to call the save changes. When we work with a repository pattern,

        // we do not like the update or add method directly inside the genetic repository. The reason is simple, when you are updating a category, logic might be different
        // than what you are doing when you are updating a product. With EF core, you only use the update statement  so it could be same as well.
        // But typically we have seen that update is more complicated  because sometimes you only want to update a few properties   or you have some other logic.
        // Because of that,we like to keep the update and save changes out of the repository,




















    }
}
