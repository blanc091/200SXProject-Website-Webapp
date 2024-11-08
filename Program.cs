using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using _200SXContact.Models.Configs;
using Microsoft.Extensions.Options;
async Task CreateRoles(IServiceProvider serviceProvider)
{
	var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
	string[] roleNames = { "Admin", "User" };
	foreach (var roleName in roleNames)
	{
		if (!await roleManager.RoleExistsAsync(roleName))
		{
			await roleManager.CreateAsync(new IdentityRole(roleName));
		}
	}
	var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
	var adminEmail = appSettings.AdminSettings.Email;
	var adminPassword = appSettings.AdminSettings.Password;
	var adminUser = await userManager.FindByEmailAsync(adminEmail);
	if (adminUser == null)
	{
		var admin = new User
		{
			UserName = "blanc0",
			Email = adminEmail,
			EmailConfirmed = true,
			CreatedAt = DateTime.Now,
			IsEmailVerified = true
		};
				
		var createAdminUserResult = await userManager.CreateAsync(admin, adminPassword);

		if (createAdminUserResult.Succeeded)
		{
			await userManager.AddToRoleAsync(admin, "Admin");
		}
		else
		{
			foreach (var error in createAdminUserResult.Errors)
			{
				Console.WriteLine($"Error creating user: {error.Description}");
			}
		}
	}
	else
	{
		if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
		{
			await userManager.AddToRoleAsync(adminUser, "Admin");
		}
	}
}

var builder = WebApplication.CreateBuilder(args);
var cultureInfo = new CultureInfo("en-US"); 
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
	options.DefaultRequestCulture = new RequestCulture(cultureInfo);
	options.SupportedCultures = new List<CultureInfo> { cultureInfo };
	options.SupportedUICultures = new List<CultureInfo> { cultureInfo };
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigins", builder =>
	{
		builder.WithOrigins("https://localhost:7109","https://200sxproject.com")
			   .AllowAnyHeader() 
			   .AllowAnyMethod() 
			   .AllowCredentials();
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
builder.Services.AddScoped<ILoggerService, LoggerService>();
builder.Services.Configure<AppSettings>(builder.Configuration);
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
app.UseRequestLocalization();
app.UseCors(builder =>
	builder.WithOrigins("https://www.google.com", "https://pagead2.googlesyndication.com", "https://ep2.adtrafficquality.google", "https://ep1.adtrafficquality.google")
		   .AllowAnyMethod()
		   .AllowAnyHeader()
);
app.UseHttpsRedirection();
app.UseStaticFiles();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Use(async (context, next) =>
{	
	var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
	logger.LogInformation($"Incoming request path: {context.Request.Path}");

	await next();
});
app.Use(async (context, next) =>
{
	var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
	context.Items["CSPNonce"] = nonce;

	var cspPolicy = $"default-src 'self'; " +
					$"script-src 'self' 'nonce-{nonce}' https://www.googletagmanager.com https://pagead2.googlesyndication.com https://aadcdn.msftauth.net https://ajax.googleapis.com https://fonts.googleapis.com https://login.microsoftonline.com https://cdnjs.cloudflare.com https://stackpath.bootstrapcdn.com https://cdn.jsdelivr.net; " +
					$"style-src 'self' https://fonts.googleapis.com 'unsafe-inline' https://stackpath.bootstrapcdn.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; " +
					$"font-src 'self' https://fonts.gstatic.com; " +
					$"img-src 'self' https://www.google.com https://image.ibb.co https://i.ibb.co/ https://ep1.adtrafficquality.google data:; " +
					$"connect-src 'self' https://login.microsoftonline.com https://aadcdn.msftauth.net http://localhost:65212 https://localhost:7109 wss://localhost:44375 http://localhost:59950 https://www.googletagmanager.com https://region1.google-analytics.com https://ep1.adtrafficquality.google ws://localhost:59950; " +
					$"frame-src 'self' https://pagead2.googlesyndication.com https://ep2.adtrafficquality.google https://ep1.adtrafficquality.google;";

	context.Response.Headers.Append("Content-Security-Policy", cspPolicy);
	await next();
});

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
	await CreateRoles(services);
}
app.Run();
