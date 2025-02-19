using MediatR;
using _200SXContact.Services;
using AutoMapper;
using _200SXContact.Models;
using _200SXContact.Interfaces.Areas.Products;
using _200SXContact.Models.Areas.Products;

namespace _200SXContact.Queries.Areas.Products
{
    public class GetAddProductInterfaceQueryHandler : IRequestHandler<GetAddProductInterfaceQuery, IProductDto>
    {
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public GetAddProductInterfaceQueryHandler(ILoggerService loggerService, IMapper mapper)
        {
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public async Task<IProductDto> Handle(GetAddProductInterfaceQuery request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Products || Getting admin add product interface", "Info", "");
            var model = new Product
            {
                Name = string.Empty,
                Category = string.Empty,
                Description = string.Empty,
                Price = 0.00m,
                ImagePaths = new List<string>(),
                DateAdded = DateTime.Now
            };
            await _loggerService.LogAsync("Products || Got admin add product interface", "Info", "");
            return _mapper.Map<IProductDto>(model);
        }
    }
}
