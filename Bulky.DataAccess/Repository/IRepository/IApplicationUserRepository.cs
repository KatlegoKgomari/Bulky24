using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bulky.DataAccess.Repository.IRepository
{ //We need our category to implement the repository
    public interface IApplicationUserRepository: IRepository<ApplicationUser> // We know the class on which we want the implementatation for this class. when we need the implementation for Icategoryrepository, we to get the base functionality from repository.
    // that will make sure we have all of these base methods where entity will be category.
    // On top of that, we needed two more functionalities and those are update and save methods.
    {
    }
}
