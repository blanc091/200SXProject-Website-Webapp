using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Mail;

namespace _200SXContact.Controllers
{
	public class ProductsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;
		private readonly ILoggerService _loggerService;
		public ProductsController(ApplicationDbContext context, ILoggerService loggerService, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
			_loggerService = loggerService;
		}
		[HttpGet]
		[Route("products/add-product-interface")]
		[Authorize(Roles = "Admin")]
		public IActionResult AddProduct()
		{
            _loggerService.LogAsync("Getting admin add product interface", "Info", "");
            var model = new Product
			{
				Name = string.Empty,
				Category = string.Empty,
				Description = string.Empty,
				Price = 0.00m,
				ImagePaths = new List<string>(),
				DateAdded = DateTime.Now
			};
            _loggerService.LogAsync("Got admin add product interface", "Info", "");
            return View("~/Views/Marketplace/AddProduct.cshtml", model);
		}
		[HttpGet]
		[Route("products/view-products")]
		public async Task<IActionResult> ProductsDashboard()
		{
            await _loggerService.LogAsync("Getting products page", "Info", "");
            var products = await _context.Products.ToListAsync();
            await _loggerService.LogAsync("Got products page", "Info", "");
            return View("~/Views/Marketplace/ProductsDashboard.cshtml", products); 
		}
		[HttpPost]
		[Route("products/add-product-admin")]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddProduct(Product model, List<IFormFile> Images)
		{
            await _loggerService.LogAsync("Adding new product admin", "Info", "");
            ModelState.Remove("ImagePaths");
			if (ModelState.IsValid)
			{
				model.DateAdded = DateTime.Now;
				if (Images != null && Images.Any())
				{
					var productDirectory = Path.Combine("wwwroot/images/products", model.Id.ToString());
					if (!Directory.Exists(productDirectory))
					{
						Directory.CreateDirectory(productDirectory);
					}
					model.ImagePaths = new List<string>();
					foreach (var image in Images)
					{
						if (image.Length > 0)
						{
							var imagePath = Path.Combine(productDirectory, image.FileName);
							using (var stream = new FileStream(imagePath, FileMode.Create))
							{
								await image.CopyToAsync(stream);
							}
							model.ImagePaths.Add($"/images/products/{model.Id}/{image.FileName}");
						}
					}
				}
				await _context.Products.AddAsync(model);
				await _context.SaveChangesAsync();
                await _loggerService.LogAsync("Added new product admin", "Info", "");
                return RedirectToAction("ProductsDashboard", "Products");
			}
			foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
			{
                await _loggerService.LogAsync("Error when adding new product admin " + error.ErrorMessage, "Error", ""); 
			}
			return View("AddProduct", model);
		}
		[HttpGet]
		[Route("products/detailed-product-view")]
		public async Task<IActionResult> DetailedProductView([FromQuery] int id)
		{
            await _loggerService.LogAsync("Getting detailed product view", "Info", "");
            var product = await _context.Products
				.FirstOrDefaultAsync(b => b.Id == id);
			if (product == null)
			{
                await _loggerService.LogAsync("No product found when trying to access detailed product view", "Error", "");
                return NotFound();
			}
            await _loggerService.LogAsync("Got detailed product view", "Info", "");
            return View("~/Views/Marketplace/DetailedProductView.cshtml", product);
		}
	}
}
