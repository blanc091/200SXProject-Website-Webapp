﻿using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Interfaces.Areas.Data;
using _200SXContact.Models.DTOs.Areas.Products;

namespace _200SXContact.Queries.Areas.Products
{
	public class GetDetailedProductViewQuery(int id) : IRequest<ProductDto?>
	{
        public int Id { get; set; } = id;
    }
    public class GetDetailedProductViewQueryHandler : IRequestHandler<GetDetailedProductViewQuery, ProductDto?>
    {
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;
        public GetDetailedProductViewQueryHandler(ILoggerService loggerService, IMapper mapper, IApplicationDbContext context)
        {
            _loggerService = loggerService;
            _mapper = mapper;
            _context = context;
        }
        public async Task<ProductDto?> Handle(GetDetailedProductViewQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Products || Getting detailed product view data", "Info", "");

            Models.Areas.Products.Product? product = await _context.Products.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (product == null)
            {
                await _loggerService.LogAsync("Products || No product found when trying to access detailed product view", "Error", "");

                return null;
            }

            await _loggerService.LogAsync("Products || Got detailed product view", "Info", "");

            return _mapper.Map<ProductDto>(product);
        }
    }
}
