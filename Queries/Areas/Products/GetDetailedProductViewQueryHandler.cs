using _200SXContact.Data;
using _200SXContact.Models.DTOs.Areas.Products;
using _200SXContact.Services;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Queries.Areas.Products
{
	public class GetDetailedProductViewQueryHandler : IRequestHandler<GetDetailedProductViewQuery, ProductDto>
	{
		private readonly ILoggerService _loggerService;
		private readonly IMapper _mapper;
		private readonly ApplicationDbContext _context;
		public GetDetailedProductViewQueryHandler(ILoggerService loggerService, IMapper mapper, ApplicationDbContext context)
		{
			_loggerService = loggerService;
			_mapper = mapper;
			_context = context;
		}
		public async Task<ProductDto> Handle(GetDetailedProductViewQuery request, CancellationToken cancellationToken)
		{
			await _loggerService.LogAsync("Products || Getting detailed product view", "Info", "");
			var product = await _context.Products
				.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

			if (product == null)
			{
				await _loggerService.LogAsync("Products || No product found when trying to access detailed product view", "Error", "");
				return new ProductDto { Id = 0 };
			}
			await _loggerService.LogAsync("Products || Got detailed product view", "Info", "");
			return _mapper.Map<ProductDto>(product);
		}
	}
}
