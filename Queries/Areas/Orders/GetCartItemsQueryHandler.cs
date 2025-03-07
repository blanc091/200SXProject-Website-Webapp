using _200SXContact.Data;
using MediatR;
using AutoMapper;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Interfaces.Areas.Admin;

public class GetCartItemsQueryHandler : IRequestHandler<GetCartItemsQuery, List<CartItem>?>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILoggerService _loggerService;
    public GetCartItemsQueryHandler(ApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
    {
        _context = context;
        _mapper = mapper;
        _loggerService = loggerService;
    }
    public async Task<List<CartItem>?> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
    {
        await _loggerService.LogAsync("Cart || Getting cart items admin", "Info", "");

        string? order = await _context.Orders.Where(o => o.Id == request.OrderId).Select(o => o.CartItemsJson).FirstOrDefaultAsync(cancellationToken);

        if (order == null)
        {
            await _loggerService.LogAsync("Cart || Order is null when getting cart items admin", "Error", "");

            return null;
        }

        List<CartItem> cartItems = _mapper.Map<List<CartItem>>(JsonSerializer.Deserialize<List<CartItem>>(order));

        await _loggerService.LogAsync("Cart || Got cart items admin", "Info", "");

        return cartItems ?? new List<CartItem>();
    }
}
