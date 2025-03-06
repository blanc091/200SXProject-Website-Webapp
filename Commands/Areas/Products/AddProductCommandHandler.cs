using MediatR;
using _200SXContact.Data;
using _200SXContact.Services;
using AutoMapper;
using FluentValidation;
using _200SXContact.Models.DTOs.Areas.Products;
using _200SXContact.Models.Areas.Products;

namespace _200SXContact.Commands.Areas.Products
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

            ProductDto productDto = request.Product;
            productDto.AddedDate = DateTime.Now.ToString();
            List<string> imagePaths = new List<string>();

            if (request.Images != null && request.Images.Any())
            {
                string uniqueImagesFolder = Guid.NewGuid().ToString();
                string productDirectory = Path.Combine(_environment.WebRootPath, "images/products", uniqueImagesFolder);

                await _loggerService.LogAsync("Products || Starting handling the images", "Info", "");

                try
                {
                    Directory.CreateDirectory(productDirectory);

                    foreach (IFormFile image in request.Images)
                    {
                        if (image.Length > 0)
                        {
                            string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                            string fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                await _loggerService.LogAsync("Products || File extensions are correct", "Info", "");

                                continue;
                            }

                            string imagePath = Path.Combine(productDirectory, image.FileName);

                            try
                            {
                                using (FileStream stream = new FileStream(imagePath, FileMode.Create))
                                {
                                    await _loggerService.LogAsync($"Products || Copying {image.FileName} to storage", "Info", "");

                                    await image.CopyToAsync(stream);

                                    await _loggerService.LogAsync($"Products || Copied {image.FileName} to storage", "Info", "");
                                }
                                imagePaths.Add($"images/products/{uniqueImagesFolder}/{image.FileName}");
                            }
                            catch (Exception ex)
                            {
                                await _loggerService.LogAsync("Products || Error while creating image folder. Please check your permissions" + ex.ToString(), "Error", "");

                                throw new ValidationException("Error while creating image folder. Please check your permissions.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _loggerService.LogAsync("Products || Error when adding product" + ex.ToString(), "Error", "");

                    throw new ValidationException("An error occurred while processing images.");
                }
            }

            productDto.ImagePaths = imagePaths;

            try
            {
                Product productEntity = _mapper.Map<Product>(productDto);

                await _context.Products.AddAsync(productEntity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await _loggerService.LogAsync("Products || Added new product admin", "Info", "");

                return true;
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync("Products || Error when adding product" + ex.ToString(), "Error", "");

                throw new ValidationException("An error occurred while saving the product, please try again.");
            }
        }
    }
}
