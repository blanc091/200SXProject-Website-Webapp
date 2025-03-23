using _200SXContact.Models.DTOs.Areas.Products;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.Products;

namespace _200SXContact.Queries.Areas.Products
{
    public class GetAddProductInterfaceQuery : IRequest<ProductDto> {}
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
                Id = 0,
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