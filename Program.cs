using _200SXContact.Data;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using _200SXContact.Models.Configs;
using Stripe;
using _200SXContact.Queries.Areas.Products;
using _200SXContact.Commands.Areas.Products;
using _200SXContact.Queries.Areas.Orders;
using _200SXContact.Commands.Areas.Orders;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Dashboard;
using _200SXContact.Commands.Areas.Newsletter;
using _200SXContact.Commands.Areas.Admin;
using _200SXContact.Queries.Areas.Newsletter;
using System.Net;
using Ganss.Xss;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Hubs;
using _200SXContact.Helpers.Areas.Chat;
using Microsoft.AspNetCore.SignalR;
using _200SXContact.Commands.Areas.Account;
using _200SXContact.Commands.Areas.MaintenApp;
using _200SXContact.Queries.Areas.UserContent;
using _200SXContact.Queries.Areas.MaintenApp;
using _200SXContact.Commands.Areas.UserContent;
using _200SXContact.Queries.Areas.Account;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Interfaces;
using _200SXContact.Helpers;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
async Task CreateRoles(IServiceProvider serviceProvider)
{
    RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
	string[] roleNames = { "Admin", "User" };
	foreach (string roleName in roleNames)
	{
		if (!await roleManager.RoleExistsAsync(roleName))
		{
			await roleManager.CreateAsync(new IdentityRole(roleName));
		}
	}
    AppSettings appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
    string adminEmail = appSettings.AdminSettings.Email;
    string adminPassword = appSettings.AdminSettings.Password;
    User? adminUser = await userManager.FindByEmailAsync(adminEmail);

	if (adminUser == null)
	{
        User admin = new User
		{
			UserName = "blanc0",
			Email = adminEmail,
			EmailConfirmed = true,
			CreatedAt = DateTime.Now,
			IsEmailVerified = true
		};
        IdentityResult createAdminUserResult = await userManager.CreateAsync(admin, adminPassword);

		if (createAdminUserResult.Succeeded)
		{
			await userManager.AddToRoleAsync(admin, "Admin");
		}
		else
		{
			foreach (IdentityError error in createAdminUserResult.Errors)
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
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
CultureInfo cultureInfo = new CultureInfo("en-US"); 
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
IConfigurationSection stripeSettingsSection = builder.Configuration.GetSection("Stripe");
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
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetProductsQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AddProductCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetAddProductInterfaceQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetDetailedProductViewQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetAllOrdersQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetCartItemsQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetCartItemsCountQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetCartItemsCheckoutQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetUserOrdersQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetOrderSummaryQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<PlaceOrderCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UpdateOrderTrackingCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AddToCartCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RemoveFromCartCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetCreateNewsletterViewQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SendNewsletterCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubscribeToNewsletterCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UnsubscribeFromNewsletterCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SendEmailCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DeleteAccountCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DeleteAccountVerifyCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateTestUserCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateEntryCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UpdateEntryCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetUserBuildsQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetUserBuildQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetUserDashboardQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitBuildCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DeleteCommentCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AddCommentCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetUserProfileQueryHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ForgotPasswordCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ResetPasswordCommandHandler>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<VerifyEmailCommandHandler>());
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoggerService, LoggerService>();
builder.Services.AddSingleton<DueDateReminderService>();
builder.Services.AddSingleton<IDueDateReminderService>(sp => sp.GetRequiredService<DueDateReminderService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<DueDateReminderService>());
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));
builder.Services.Configure<StripeSettings>(stripeSettingsSection);
builder.Services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IClientTimeProvider, ClientTimeProvider>();
builder.Services.AddTransient(typeof(ClientTimeResolver<,>));
builder.Services.AddSingleton<NetworkCredential>(sp =>
{
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
    string? username = configuration["EmailCredentials:UserName"];
    string? password = configuration["EmailCredentials:Password"];

    return new NetworkCredential(username, password);
});
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        string clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, partition =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 6,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
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
WebApplication app = builder.Build();
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
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Use(async (context, next) =>
{
    ILogger<Program> logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
	logger.LogInformation($"Incoming request path: {context.Request.Path}");
	await next();
});
app.Use(async (context, next) =>
{
    string nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
	context.Items["CSPNonce"] = nonce;
    string cspPolicy = $"default-src 'self'; " +
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
                    "https://pagead2.googlesyndication.com ws://localhost:59950 http://localhost:50747 ws://localhost:50747 wss://localhost:44393;" +
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
app.MapControllerRoute(
	name: "account", 
	pattern: "account/{action}/{id?}",
	defaults: new { controller = "Account" });
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
app.MapPost("/logout", async context =>
{
	await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
	await context.SignOutAsync(MicrosoftAccountDefaults.AuthenticationScheme);
	context.Response.Redirect("/");
});
app.MapHub<ChatHub>("/livechat");
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
	await CreateRoles(services);
}
app.Run();
