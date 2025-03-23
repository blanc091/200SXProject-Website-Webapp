using _200SXContact.Queries.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;
using _200SXContact.Commands.Areas.Orders;
using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.Areas.UserContent;
using _200SXContact.Interfaces.Areas.Data;

namespace _200SXContact.Controllers.Areas.Orders
{
    public class CartController : Controller
	{
		private readonly IApplicationDbContext _context;
		private readonly UserManager<User> _userManager;
		private readonly ILoggerService _loggerService;
		private readonly IMediator _mediator;
        private readonly IMapper _mapper;
		public CartController(IApplicationDbContext context, UserManager<User> userManager, ILoggerService loggerService, IMediator mediator, IMapper mapper)
		{
			_context = context;
			_userManager = userManager;
			_loggerService = loggerService;
			_mediator = mediator;
            _mapper = mapper;
		}
        [HttpGet]
        [Route("cart/view-cart")]
        public async Task<IActionResult> CartView()
        {
            await _loggerService.LogAsync("Checkout || Getting cart view", "Info", "");

            User? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                await _loggerService.LogAsync("Checkout || User is not logged in when viewing cart", "Error", "");

                TempData["Message"] = "You need to be registered and logged in to view your cart !";

                return Redirect("/login-page");
            }

            List<CartItemDto?>? cartItems = await _mediator.Send(new GetCartItemsCheckoutQuery(user.Id));

            if (cartItems is null)
            {
                await _loggerService.LogAsync("Checkout || The cart is empty in checkout", "Info", "");

                TempData["Message"] = "Cart is empty !";
            }

            await _loggerService.LogAsync("Checkout || Got cart view", "Info", "");

            return View("~/Views/Marketplace/CartView.cshtml", cartItems);
        }
        [HttpPost]
		[Route("cart/add-item")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            User? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _loggerService.LogAsync("Checkout || User is not logged in when adding to cart", "Error", "");

                TempData["IsUserLoggedIn"] = "no";
                TempData["Message"] = "You need to be registered and logged in to add products to your cart.";

                return Redirect("/login-page");
            }

            CartItemDto cartItemDto = await _mediator.Send(new AddToCartCommand(productId, quantity, user.Id));

            if (cartItemDto == null)
            {
                await _loggerService.LogAsync("Cart || Product not found or failed to add to cart", "Error", "");

                TempData["Message"] = "Product not found or failed to add to cart.";
            }
            else
            {
                await _loggerService.LogAsync("Cart || Added item to cart", "Info", "");

                TempData["ItemAdded"] = "yes";
            }

            return RedirectToAction("ProductsDashboard", "Products");
        }
        [HttpGet]
		[Route("cart/get-cart-items")]
        public async Task<IActionResult> GetCartItemCount()
        {
            await _loggerService.LogAsync("Checkout || Getting cart items count", "Info", "");

            User? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                await _loggerService.LogAsync("Checkout || User is null when getting cart items", "Error", "");

                return Json(0);
            }

            int cartItemCount = await _mediator.Send(new GetCartItemsCountQuery { UserId = user.Id });

            if (cartItemCount == 0)
            {
                await _loggerService.LogAsync("Checkout || No cart items found to fetch for user " + user.Id.ToString(), "Error", "");

                return Json(0);
            }

            return Json(cartItemCount);
        }
        [HttpPost]
		[Route("cart/remove-cart-item")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            await _loggerService.LogAsync("Checkout || Removing cart item", "Info", "");

            User? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                await _loggerService.LogAsync("Checkout || User is null when removing cart items", "Error", "");

                return Json(0);
            }

            bool success = await _mediator.Send(new RemoveFromCartCommand { ProductId = productId, UserId = user.Id });

            if (!success)
            {
                await _loggerService.LogAsync("Checkout || Cart item not found when removing cart item", "Error", "");

                return NotFound();
            }

            List<Models.Areas.Orders.CartItem> cartItems = await _context.CartItems.Where(ci => ci.UserId == user.Id).ToListAsync();

            List<CartItemDto> cartItemDtos = _mapper.Map<List<CartItemDto>>(cartItems);

            await _loggerService.LogAsync("Checkout || Removed cart item", "Info", "");

            return View("~/Views/Marketplace/CartView.cshtml", cartItemDtos);
        }
    }
}