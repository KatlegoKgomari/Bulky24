using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class //We are implementing a generic interface so the class needs to be generic as well
    {
        //When we added, we used to say _db.add() so we will need that using dependency injection
        private readonly ApplicationDbContext _db;
        // Wewant to make sure to use generic so that way, we can access db set directly. So we create an internal dbSet on the generic type T 
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>(); // We can set the current one, that is the generic one. that we get here, and that will be set to the dbSet. So, when we create this generic class on category, the dbSet will set to Categories.
            //_db.Categories = dbSet;
            _db.Products.Include(u => u.Category); // Category will automatically be populated when it retrieves all the products based on the foreign key relation
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);  
        }
        // If soemone gives us Category or covertype, based on that, we can get the input properties. If given to us as a comma separated string, then we can include it
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties= null)
        {

            IQueryable<T> query = dbSet;
            if (filter != null) 
            {
                query = query.Where(filter);
            }
            
            if (!string.IsNullOrEmpty(includeProperties))
            {

                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) //If any entry is empty, it will remove it
                {
                    query = query.Include(includeProp);
                }

            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                // Individual get. We will be working on an Iqueriable of T
                query = dbSet; //Assigning the complete db set
                
            }
            else
            {
                query = dbSet.AsNoTracking(); //ensures that ef core does not track the entity
            }

            query = query.Where(filter); // On the query, we can apply a where condition an apply the filter. Whatever we get, we assign back to the query
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) //If any entry is empty, it will remove it
                {
                    query = query.Include(includeProp);
                }
            }

            return query.FirstOrDefault(); // we return a set of the objects that meet the condition
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity); //We have a built in method called remove range. inside EF Core. That RemoveRange expects an IEnumerable and it will remove all of those Categories that are passed to the IEnumerable.




        }
    }
}
