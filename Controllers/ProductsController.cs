using _200SXContact.Commands;
using _200SXContact.Data;
using _200SXContact.Interfaces.Areas.Products;
using _200SXContact.Models;
using _200SXContact.Models.Areas.Products;
using _200SXContact.Models.DTOs.Areas.Products;
using _200SXContact.Queries;
using _200SXContact.Queries.Areas.Products;
using _200SXContact.Services;
using MediatR;
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
		private readonly IMediator _mediator;
		public ProductsController(ApplicationDbContext context, ILoggerService loggerService, UserManager<User> userManager, IMediator mediator)
		{
			_context = context;
			_userManager = userManager;
			_loggerService = loggerService;
			_mediator = mediator;
		}
		[HttpGet]
		[Route("products/add-product-interface")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddProduct()
		{
			var query = new GetAddProductInterfaceQuery();
			ProductDto productDto = await _mediator.Send(query);
			return View("~/Views/Marketplace/AddProduct.cshtml", productDto);
		}
		[HttpGet]
		[Route("products/view-products")]
		public async Task<IActionResult> ProductsDashboard()
		{
			List<ProductDto> products = await _mediator.Send(new GetProductsQuery());
            return View("~/Views/Marketplace/ProductsDashboard.cshtml", products); 
		}
		[HttpPost]
		[Route("products/add-product-admin")]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddProduct(ProductDto products, List<IFormFile> Images)
		{
			bool result = await _mediator.Send(new AddProductCommand(products, Images));
			if (result)
			{
				return RedirectToAction("ProductsDashboard", "Products");
			}
			return View("AddProduct", products);
		}
		[HttpGet]
		[Route("products/detailed-product-view")]
		public async Task<IActionResult> DetailedProductView([FromQuery] int id)
		{
			await _loggerService.LogAsync("Products || Getting detailed product view", "Info", "");
			var query = new GetDetailedProductViewQuery(id);
			var product = await _mediator.Send(query);
			if (product.Id == 0)
			{
				await _loggerService.LogAsync("Products || No product found when trying to access detailed product view", "Error", "");
				return NotFound();
			}
			await _loggerService.LogAsync("Products || Got detailed product view", "Info", "");
			return View("~/Views/Marketplace/DetailedProductView.cshtml", product);
		}
	}
}
