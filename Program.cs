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
using static System.Net.WebRequestMethods;
using Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.OAuth;
using _200SXContact.Interfaces;
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
var stripeSettingsSection = builder.Configuration.GetSection("Stripe");
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
		builder.WithOrigins("https://preprod.200sxproject.com", "https://localhost:7109","https://200sxproject.com", "https://*.stripe.com", "https://*.hcaptcha.com")
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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoggerService, LoggerService>();
builder.Services.AddSingleton<DueDateReminderService>();
builder.Services.AddSingleton<IDueDateReminderService>(sp => sp.GetRequiredService<DueDateReminderService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<DueDateReminderService>());

builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));
builder.Services.Configure<StripeSettings>(stripeSettingsSection);
StripeConfiguration.ApiKey = stripeSettingsSection["SecretKey"];
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/login-page";
		options.LogoutPath = "/loginregister/logout";
		options.AccessDeniedPath = "/access-denied";
		options.Cookie.Name = "_200SXContact.AuthCookie";          // prod
		//options.Cookie.Name = "_200SXContact.AuthCookie_PreProd";	 // preprod
		options.Cookie.HttpOnly = false;
		options.Cookie.Domain = ".200sxproject.com"; // prod, preprod
		options.SlidingExpiration = true;
		//options.Cookie.SecurePolicy = CookieSecurePolicy.None;		 // preprod
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always;   // prod
		options.Cookie.SameSite = SameSiteMode.Strict;			 // prod
		//options.Cookie.SameSite = SameSiteMode.Lax;					 // preprod
		options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
	})
.AddMicrosoftAccount(microsoftOptions =>
{
	microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
	microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
	microsoftOptions.CallbackPath = "/sign-in-microsoft";	
});
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseDeveloperExceptionPage();
	app.UseHsts();
}
app.UseRouting();
app.UseRequestLocalization();
app.UseCors(builder =>
	builder.WithOrigins("https://www.google.com", "https://pagead2.googlesyndication.com", "https://ep2.adtrafficquality.google", "https://ep1.adtrafficquality.google", "https://www.paypal.com", "https://www.sandbox.paypal.com", "https://js.stripe.com", "https://newassets.hcaptcha.com")
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
					$"script-src 'self' 'nonce-{nonce}' " +
					"https://www.googletagmanager.com " +
					"https://pagead2.googlesyndication.com " +
					"https://aadcdn.msftauth.net " +
					"https://*.googleapis.com " +
					"http://*.200sxproject.com " +
					"https://login.microsoftonline.com " +
					"https://cdnjs.cloudflare.com " +
					"https://stackpath.bootstrapcdn.com " +
					"https://cdn.jsdelivr.net " +
					"https://www.google-analytics.com " +
					"https://ep2.adtrafficquality.google " +
					"https://*.stripe.network " +
					"https://cdn.jsdelivr.net/npm/swiper@11/swiper-bundle.min.js " +
					"https://*.paypal.com " +
					"https://www.paypalobjects.com " +
					"blob: https://newassets.hcaptcha.com https://*.hcaptcha.com " +
					"https://*.stripe.com " +
					"https://www.google.com/recaptcha/api.js " +
					"https://www.gstatic.com/recaptcha/; " +
					$"style-src 'self' https://fonts.googleapis.com 'unsafe-inline' " +
					"https://stackpath.bootstrapcdn.com " +
					"https://cdnjs.cloudflare.com " +
					"https://cdn.jsdelivr.net " +
					"https://*.google.com " +
					"https://cdn.jsdelivr.net/npm/swiper@11/swiper-bundle.min.css; " +
					$"font-src 'self' https://fonts.gstatic.com data:; " +
					$"img-src 'self' https://www.google.com https://*.ibb.co/ " +
					"https://www.paypalobjects.com " +
					"https://*.adtrafficquality.google https://pagead2.googlesyndication.com data: https://www.gstatic.com/recaptcha/; " +
					$"connect-src 'self' https://login.microsoftonline.com https://aadcdn.msftauth.net " +
					"https://www.googletagmanager.com https://*.paypal.com http://*.200sxproject.com " +
					"https://region1.google-analytics.com https://*.adtrafficquality.google https://*.stripe.com https://*.hcaptcha.com " +
					"https://*.google.com " +
					"https://pagead2.googlesyndication.com ws://localhost:59950; " +
					$"frame-src 'self' https://pagead2.googlesyndication.com " +
					"https://*.paypal.com " +
					"https://*.stripe.com " +
					"https://*.hcaptcha.com " +
					"https://*.google.com " + 
					"https://*.adtrafficquality.google;";

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
app.MapControllerRoute(
	name: "pendingorders",
	pattern: "pendingorders/{action}/{id?}",
	defaults: new { controller = "PendingOrders" });
app.MapPost("/logout", async context =>
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
