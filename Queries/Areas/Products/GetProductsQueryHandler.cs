using _200SXContact.Data;
using _200SXContact.Models;
using _200SXContact.Models.DTOs.Areas.Products;
using _200SXContact.Services;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Queries.Areas.Products
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public GetProductsQueryHandler(ApplicationDbContext context, ILoggerService loggerService, IMapper mapper)
        {
            _context = context;
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Products || Fetching product list", "Info", "");
            var products = await _context.Products.ToListAsync(cancellationToken);
            if (products.Count == 0)
            {
                await _loggerService.LogAsync("Products || No products found", "Warning", "");
            }
            else
            {
                await _loggerService.LogAsync($"Products || Retrieved {products.Count} products", "Info", "");
            }
            await _loggerService.LogAsync($"Products || Retrieved {products.Count} products", "Info", "");
            return _mapper.Map<List<ProductDto>>(products);
        }
    }
}
