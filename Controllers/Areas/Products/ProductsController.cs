﻿using _200SXContact.Commands.Areas.Products;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Products;
using _200SXContact.Queries.Areas.Products;

namespace _200SXContact.Controllers.Areas.Products
{
    public class ProductsController : Controller
	{
		private readonly ILoggerService _loggerService;
		private readonly IMediator _mediator;
		public ProductsController(ILoggerService loggerService, IMediator mediator)
		{
			_loggerService = loggerService;
			_mediator = mediator;
		}
		[HttpGet]
		[Route("products/add-product-interface")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddProduct()
		{
			ProductDto productDto = await _mediator.Send(new GetAddProductInterfaceQuery());

			return View("~/Views/Marketplace/AddProduct.cshtml", productDto);
		}
		[HttpGet]
		[Route("products/view-products")]
		public async Task<IActionResult> ProductsDashboard()
		{
			List<ProductDto?>? products = await _mediator.Send(new GetProductsQuery());
			
			if (products == null)
			{
                await _loggerService.LogAsync("Products || No products found currently", "Info", "");

                TempData["Message"] = "No products added at this point.";
			}

			return View("~/Views/Marketplace/ProductsDashboard.cshtml", products); 
		}
		[HttpPost]
		[Route("products/add-product-admin")]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductDto products, List<IFormFile> Images)
        {
            try
            {
                bool result = await _mediator.Send(new AddProductCommand(products, Images));

                if (result)
                {
                    return RedirectToAction("ProductsDashboard", "Products");
                }

                return View("AddProduct", products);
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync("Product || Error adding product: " + ex.Message, "Error", "");

                return View("AddProduct", products);
            }
        }
        [HttpGet]
		[Route("products/detailed-product-view")]
		public async Task<IActionResult> DetailedProductView([FromQuery] int id)
		{
            ProductDto? product = await _mediator.Send(new GetDetailedProductViewQuery(id));
            
			if (product == null)
            {
                await _loggerService.LogAsync("Products || No product found when trying to access detailed product view", "Error", "");

				TempData["Message"] = "No product found when trying to get detailed view !";
            }           

			return View("~/Views/Marketplace/DetailedProductView.cshtml", product);
		}
	}
}
