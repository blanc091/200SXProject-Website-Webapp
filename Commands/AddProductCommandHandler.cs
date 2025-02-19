using MediatR;
using _200SXContact.Data;
using _200SXContact.Services;
using AutoMapper;
using _200SXContact.Models.Areas.Products;

namespace _200SXContact.Commands
{
	public class AddProductCommandHandler : IRequestHandler<AddProductCommand, bool>
	{
		private readonly ApplicationDbContext _context;
		private readonly ILoggerService _loggerService;
		private readonly IWebHostEnvironment _environment;
		private readonly IMapper _mapper;
		public AddProductCommandHandler(ApplicationDbContext context, ILoggerService loggerService, IWebHostEnvironment environment, IMapper mapper)
		{
			_context = context;
			_loggerService = loggerService;
			_environment = environment;
			_mapper = mapper;
		}
		public async Task<bool> Handle(AddProductCommand request, CancellationToken cancellationToken)
		{
			await _loggerService.LogAsync("Products || Adding new product admin", "Info", "");
			var product = _mapper.Map<Product>(request.Product);
			product.DateAdded = DateTime.Now;
			if (request.Images != null && request.Images.Any())
			{
				var productDirectory = Path.Combine(_environment.WebRootPath, "images/products", product.Id.ToString());
				if (!Directory.Exists(productDirectory))
				{
					Directory.CreateDirectory(productDirectory);
				}
				List<string> imagePaths = new List<string>();
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
