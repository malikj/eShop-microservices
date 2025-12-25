namespace Catalog.Application.Checkout.Dtos;

public record CheckoutItemDto(
	Guid ProductId,
	int Quantity
);

