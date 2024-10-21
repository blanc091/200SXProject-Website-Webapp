using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


async Task CreateRoles(IServiceProvider serviceProvider)
{
	var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

	string[] roleNames = { "Admin", "User" };
	IdentityResult roleResult;

	foreach (var roleName in roleNames)
	{
		var roleExist = await roleManager.RoleExistsAsync(roleName);
		if (!roleExist)
		{
			// Create the roles if they do not exist
			roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
		}
	}

	// Create an admin user if not already created
	var adminUser = await userManager.FindByEmailAsync("mircea.albu91@gmail.com");
	if (adminUser == null)
	{
		var admin = new User
		{
			UserName = "blanc0",
			Email = "mircea.albu91@gmail.com",
			EmailConfirmed = true,
			CreatedAt = DateTime.Now, // Ensure CreatedAt is set
			IsEmailVerified = true // or false, depending on your requirements
		};

		string adminPassword = "Recall1547!";
		var createAdminUser = await userManager.CreateAsync(admin, adminPassword); // No need to set PasswordHash

		if (createAdminUser.Succeeded)
		{
			// Assign Admin role to the admin user
			await userManager.AddToRoleAsync(admin, "Admin");
		}
	}
}
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddIdentity<User, IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(15);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/LoginRegister/Login";
		options.LogoutPath = "/LoginRegister/Logout";
		options.Cookie.Name = "_200SXContact.AuthCookie";
		options.Cookie.HttpOnly = true;
		options.SlidingExpiration = true;
		options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set the expiration time
	})
.AddMicrosoftAccount(microsoftOptions =>
{
	microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
	microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
	microsoftOptions.CallbackPath = "/signin-microsoft"; 
});
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
var logger = app.Services.GetRequiredService<ILogger<Program>>(); // Use Program as the logger category
app.Use(async (context, next) =>
{
	var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
	logger.LogInformation($"Incoming request path: {context.Request.Path}");
	await next();
});
//app.Use(async (context, next) =>
//{
//	var cspPolicy = "default-src 'self'; " +
// 					"script-src 'self' https://ajax.googleapis.com https://fonts.googleapis.com https://login.microsoftonline.com https://aadcdn.msftauth.net https://stackpath.bootstrapcdn.com; " +
// 					"style-src 'self' https://fonts.googleapis.com 'unsafe-inline'; " +
// 					"img-src 'self' https://www.google.com data:; " +
// 					"font-src 'self' https://fonts.gstatic.com; " +
// 					"frame-ancestors 'none'; " +
// 					"connect-src 'self' https://login.microsoftonline.com https://aadcdn.msftauth.net http://localhost:65212 https://localhost:7109;";

//	context.Response.Headers.Append("Content-Security-Policy", cspPolicy);
//	await next();
//});
app.UseRouting();
app.UseSession();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapPost("/signout", async context =>
{
	await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
	await context.SignOutAsync(MicrosoftAccountDefaults.AuthenticationScheme);
	context.Response.Redirect("/");
});
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	await CreateRoles(services);
}
app.Run();
