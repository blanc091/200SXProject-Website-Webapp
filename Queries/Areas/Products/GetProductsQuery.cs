using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.DTOs.Areas.Products;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Queries.Areas.Products
{
    public class GetProductsQuery : IRequest<List<ProductDto?>?> {}
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto?>?>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public GetProductsQueryHandler(IApplicationDbContext context, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<List<ProductDto?>?> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Products || Fetching product list", "Info", "");

            List<Models.Areas.Products.Product> products = await _context.Products.ToListAsync(cancellationToken);

            if (products.Count == 0)
            {
                await _loggerService.LogAsync("Products || No products found", "Warning", "");

                return null;
            }
            else
            {
                await _loggerService.LogAsync($"Products || Retrieved {products.Count} products", "Info", "");
            }

            return _mapper.Map<List<ProductDto?>>(products);
        }
    }
}
