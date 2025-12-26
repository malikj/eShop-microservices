using Catalog.Application.Abstractions.Messaging;
using Catalog.Application.Checkout.Dtos;
using Catalog.Domain.Repositories;
using eShop.Contracts.Events;

namespace Catalog.Application.Checkout;

public class CheckoutService : ICheckoutService
{
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;

    public CheckoutService(
        IProductRepository productRepository,
        IEventPublisher eventPublisher)
    {
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> CheckoutAsync(CheckoutRequestDto request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("Checkout must contain at least one item");

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds);

        if (products.Count != productIds.Count)
            throw new InvalidOperationException("One or more products not found");

        var orderItems = products.Select(p =>
        {
            var quantity = request.Items
                .First(i => i.ProductId == p.Id)
                .Quantity;

            return new OrderItemDto(
                p.Id,
                p.Name,
                p.Price,
                quantity
            );
        }).ToList();

        var totalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity);
        var orderId = Guid.NewGuid();

        var orderRequested = new OrderRequested(
            orderId,
            request.CustomerId,
            DateTime.UtcNow,
            totalAmount,
            orderItems
        );
     await _eventPublisher.PublishOrderRequestedAsync(orderId, request.CustomerId,DateTime.UtcNow,totalAmount,orderItems);
     return orderId;
    }
}

