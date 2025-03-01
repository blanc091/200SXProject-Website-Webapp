using MediatR;
using _200SXContact.Services;
using AutoMapper;
using _200SXContact.Models.Areas.Products;
using _200SXContact.Models.DTOs.Areas.Products;

namespace _200SXContact.Queries.Areas.Products
{
    public class GetAddProductInterfaceQueryHandler : IRequestHandler<GetAddProductInterfaceQuery, ProductDto>
    {
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public GetAddProductInterfaceQueryHandler(ILoggerService loggerService, IMapper mapper)
        {
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<ProductDto> Handle(GetAddProductInterfaceQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Products || Getting admin add product interface", "Info", "");

            Product model = new Product
            {
                Name = string.Empty,
                Category = string.Empty,
                Description = string.Empty,
                Price = 0.00m,
                ImagePaths = new List<string>(),
                DateAdded = DateTime.Now
            };

            await _loggerService.LogAsync("Products || Got admin add product interface", "Info", "");

            return _mapper.Map<ProductDto>(model);
        }
    }
}
