using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public DBInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db =db;
        }

        public void Initialize() //We are seeding data with this method
        {
            //migrations if they are not applied. So this part may ecxecute mny times
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate(); 
                }
            }
            catch ( Exception ex)
            {

            }

            //create roles if they ae not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult()) // saying GetAwaiter().GetResult() is the same as saying "await"
            { //If it exists, we don't do anything but if it any of the roles does not exist, then we want to create all the roles
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult(); // We do not have to save chnages, the helper method takes are of everything (we also don't need to use the unit of work)
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                //If roles are not created, the we will create admin user as well
                //Creating an Application User and populating some of the values
                _userManager.CreateAsync(new ApplicationUser //Application, password
                {
                    UserName = "katlego.kgomari@24.com",
                    Email = "katlego.kgomari@24.com",
                    Name = "Katlego Kgomari",
                    PhoneNumber = "0987654321",
                    StreetAddress = "123 BombShell Street",
                    PostalCode = "1234",
                    State = "La La Land",
                    City = "BroomStick"
                }, "Admin123*").GetAwaiter().GetResult();  //We use these methods becase we are in an async method

                //Once user is created, we retrieve thatuser from the database
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "kgomari88@gmail.com"); //Notethat we retrieve the user using _db
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

            }
            return; //Returning back to the application
            
        }
    }
}
