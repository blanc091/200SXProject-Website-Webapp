using MediatR;
using _200SXContact.Data;
using _200SXContact.Services;

namespace _200SXContact.Commands
{
	public class AddProductCommandHandler : IRequestHandler<AddProductCommand, bool>
	{
		private readonly ApplicationDbContext _context;
		private readonly ILoggerService _loggerService;
		private readonly IWebHostEnvironment _environment;
		public AddProductCommandHandler(ApplicationDbContext context, ILoggerService loggerService, IWebHostEnvironment environment)
		{
			_context = context;
			_loggerService = loggerService;
			_environment = environment;
		}
		public async Task<bool> Handle(AddProductCommand request, CancellationToken cancellationToken)
		{
			await _loggerService.LogAsync("Products || Adding new product admin", "Info", "");
			var product = request.Product;
			product.DateAdded = DateTime.Now;
			if (request.Images != null && request.Images.Any())
			{
				var productDirectory = Path.Combine(_environment.WebRootPath, "images/products", product.Id.ToString());
				if (!Directory.Exists(productDirectory))
				{
					Directory.CreateDirectory(productDirectory);
				}
				product.ImagePaths = new List<string>();
				foreach (var image in request.Images)
				{
					if (image.Length > 0)
					{
						var imagePath = Path.Combine(productDirectory, image.FileName);
						using (var stream = new FileStream(imagePath, FileMode.Create))
						{
							await image.CopyToAsync(stream);
						}
						product.ImagePaths.Add($"/images/products/{product.Id}/{image.FileName}");
					}
				}
			}
			await _context.Products.AddAsync(product, cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);
			await _loggerService.LogAsync("Products || Added new product admin", "Info", "");
			return true;
		}
	}
}
