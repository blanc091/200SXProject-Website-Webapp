using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.Areas.Products;

namespace _200SXContact.Commands.Areas.Products
{  
    public class DeleteProductCommand(int productId) : IRequest<bool>
    {
        public int ProductId { get; set; } = productId;
    }
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IWebHostEnvironment _environment;
        public DeleteProductCommandHandler(IApplicationDbContext context, ILoggerService loggerService, IWebHostEnvironment environment)
        {
            _context = context;
            _loggerService = loggerService;
            _environment = environment;
        }
        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync($"Products || Attempting to delete product with ID: {request.ProductId}", "Info", "");

            Product? product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);

            if (product == null)
            {
                await _loggerService.LogAsync($"Products || Product with ID {request.ProductId} not found.", "Warning", "");

                return false;
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync(cancellationToken);

                await _loggerService.LogAsync($"Products || Successfully deleted product with ID {request.ProductId}.", "Info", "");

                if (product.ImagePaths != null && product.ImagePaths.Any())
                {
                    string firstImagePath = product.ImagePaths.First();
                    string prefix = "/images/products/";

                    if (firstImagePath.StartsWith(prefix))
                    {
                        string remaining = firstImagePath.Substring(prefix.Length);
                        string folderName = remaining.Split('/')[0];
                        string folderPath = Path.Combine(_environment.WebRootPath, "images", "products", folderName);

                        if (Directory.Exists(folderPath))
                        {
                            Directory.Delete(folderPath, recursive: true);

                            await _loggerService.LogAsync($"Products || Deleted image folder: {folderPath}", "Info", "");
                        }
                        else
                        {
                            await _loggerService.LogAsync($"Products || Folder not found: {folderPath}", "Warning", "");
                        }
                    }
                    else
                    {
                        await _loggerService.LogAsync("Products || First image path did not start with expected prefix.", "Warning", "");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await _loggerService.LogAsync($"Products || Error deleting product with ID {request.ProductId}: {ex.Message}", "Error", "");

                return false;
            }
        }
    }
}