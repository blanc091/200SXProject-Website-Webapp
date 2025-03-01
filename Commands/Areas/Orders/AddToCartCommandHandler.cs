﻿using _200SXContact.Data;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.DTOs.Areas.Orders;
using _200SXContact.Services;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using _200SXContact.Models.Areas.Products;

namespace _200SXContact.Commands.Areas.Orders
    {
        public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartItemDto?>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;
            private readonly ILoggerService _loggerService;
            public AddToCartCommandHandler(ApplicationDbContext context, IMapper mapper, ILoggerService loggerService)
            {
                _context = context;
                _mapper = mapper;
                _loggerService = loggerService;
            }
        public async Task<CartItemDto?> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            await _loggerService.LogAsync("Cart || Starting adding item to cart", "Info", "");

            Product? product = await _context.Products.FindAsync(request.ProductId);

            if (product == null)
            {
                await _loggerService.LogAsync("Cart || Error adding item to cart, product not linked", "Error", "");

                return null;
            }

            await _loggerService.LogAsync("Cart || Getting cart items from DB", "Info", "");

            CartItemModel? userCartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.ProductId == request.ProductId && ci.UserId == request.UserId);

            if (userCartItem != null)
            {
                await _loggerService.LogAsync("Cart || No cart items found in DB for user " + request.UserId, "Error", "");

                userCartItem.Quantity += request.Quantity;
            }
            else
            {
                await _loggerService.LogAsync("Cart || Got cart items from DB", "Info", "");

                string primaryImagePath = product.ImagePaths?.FirstOrDefault() ?? "/images/default-placeholder.png";
                CartItemModel newCartItem = new CartItemModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = request.Quantity,
                    ImagePath = primaryImagePath,
                    UserId = request.UserId
                };

                await _loggerService.LogAsync("Cart || Updating cart items model", "Info", "");

                await _context.CartItems.AddAsync(newCartItem, cancellationToken);
                userCartItem = newCartItem;
            }

            await _context.SaveChangesAsync(cancellationToken);

            CartItemDto cartItemDto = _mapper.Map<CartItemDto>(userCartItem);

            await _loggerService.LogAsync("Cart || Finished adding item to cart", "Info", "");

            return cartItemDto;
        }
    }
}

