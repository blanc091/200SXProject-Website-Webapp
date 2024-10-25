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

	// Create roles if they don't exist
	string[] roleNames = { "Admin", "User" };
	foreach (var roleName in roleNames)
	{
		if (!await roleManager.RoleExistsAsync(roleName))
		{
			await roleManager.CreateAsync(new IdentityRole(roleName));
		}
	}

	// Check if the admin user exists
	var adminEmail = "mircea.albu91@gmail.com"; // Your admin email
	var adminUser = await userManager.FindByEmailAsync(adminEmail);

	if (adminUser == null)
	{
		// Create a new admin user
		var admin = new User
		{
			UserName = "blanc0",  // Username can be adjusted as needed
			Email = adminEmail,
			EmailConfirmed = true,
			CreatedAt = DateTime.Now,
			IsEmailVerified = true
		};

		string adminPassword = "Recall1547!"; // Ensure you have a strong password here
		var createAdminUserResult = await userManager.CreateAsync(admin, adminPassword);

		if (createAdminUserResult.Succeeded)
		{
			await userManager.AddToRoleAsync(admin, "Admin");
		}
		else
		{
			// Log the errors if the user creation fails
			foreach (var error in createAdminUserResult.Errors)
			{
				Console.WriteLine($"Error creating user: {error.Description}");
			}
		}
	}
}

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigins", builder =>
	{
		builder.WithOrigins("https://localhost:7109","https://200sxproject.com") // Add specific allowed origins here
			   .AllowAnyHeader() // Allows any header
			   .AllowAnyMethod() // Allows any HTTP method
			   .AllowCredentials(); // Allows cookies and other credentials
	});
});
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
	//options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
	options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._ ";
	options.User.RequireUniqueEmail = true;

})

	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
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
		options.AccessDeniedPath = "/LoginRegister/AccessDenied";
		options.Cookie.Name = "_200SXContact.AuthCookie";
		options.Cookie.HttpOnly = true;		
		options.SlidingExpiration = true;
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use HTTPS
		options.Cookie.SameSite = SameSiteMode.Strict; // Prevent CSRF attacks
		options.ExpireTimeSpan = TimeSpan.FromMinutes(60); 
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
app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseStaticFiles();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Use(async (context, next) =>
{	
	// Log the incoming request path for other requests
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

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
app.UseSession();
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
	//await CreateRoles(services);
}
app.Run();
