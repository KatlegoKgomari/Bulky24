using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Bulky.DataAccess.DBInitializer;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Remember to say that there will be razor pages

// We tell our application that we want to use Entity Framework Core (add it to the project)
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));//This is the DBContext that is there in entity framework core. We also have to say which class has the implementation of db context

//We can configure it such that secrets in appsettings.json are automatically injected inside the SecreKey and Publishable key properties
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe")); //as long as names match, it will automatically be injected into the properties


builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders(); //When we add Identity to the project, it basically also adds all the database tables that are needed for the Identity, like we have users table, we have roles table, we have user roles table.
// all of those tables will be managed with the help of DbContext which is ApplicationDbContext. That is where we are binding Entity Framework with the Identity tables.
// It was originally adding just identity user, it was not adding role to the identity that we have in our project. so to have the role, we say addIdentity and we also add identityRole

// options goes to and then we specify what the options we want to configure are. The UseSqlServer is found inside the entity framework  core nuget package
//When we ae using the sql server, we have to define the connection string right here
builder.Services.AddScoped<IDBInitializer, DBInitializer>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); //  We want a scoped lifetime for it .so when we want the implementation in the controller, the prgram now that I have to give od category repository
// We no longer require the CategoryRepository. unitOfWork internally creates an object or implementation of categoryRepository.
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.ConfigureApplicationCookie(options => //Added this because we want these specific error pages to show
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddDistributedMemoryCache(); //Adding session to the services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "1101060204569551";
    option.AppSecret = "6170c7e9af2d2a8e04b5b615f5607158";
});

builder.Services.AddAuthentication().AddMicrosoftAccount(option =>
{
    option.ClientId = "1b4f5b46-b363-4de8-a3a0-87878fbea06a";
    option.ClientSecret = "JB.8Q~XtQDr-klco-WhiDFqbAmRAfgq9TQHWuae2";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); //Authentication muct be added before authorization. If the username and password are valid, then the authorization comes into the picture
app.UseAuthorization();  //In order to check the role of a user, they must first be authenticated
//configuring the secret key for stripe
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseSession(); //Adding session to request pipeline
seedDatabase();
app.MapRazorPages(); // //This makes sure that it adds the routing that is needed to map the razor pages
//This is where we have the default route (this is an action method)
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}"); //If nothing gets defined, we should go to the home controller. And here, we have index action
//If nothing is defined after the domain name, it is telling the application that it should go to the home controller and the index action method. 
//Basically, if you just have the domain name and nothing else, this specifies what page your app should land on

app.Run();


void seedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>(); //We are telling sercvice provider to get an implementation of our service provider so we needto register it in builder.services
        dbInitializer.Initialize();
    }
}