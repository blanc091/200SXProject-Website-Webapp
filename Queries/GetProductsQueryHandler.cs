using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Queries
{
	public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<Product>>
	{
		private readonly ApplicationDbContext _context;
		private readonly ILoggerService _loggerService;
		public GetProductsQueryHandler(ApplicationDbContext context, ILoggerService loggerService)
		{
			_context = context;
			_loggerService = loggerService;
		}
		public async Task<List<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
		{
			await _loggerService.LogAsync("Products || Fetching product list", "Info", "");
			var products = await _context.Products.ToListAsync(cancellationToken);
			await _loggerService.LogAsync($"Products || Retrieved {products.Count} products", "Info", "");
			return products;
		}
	}
}
