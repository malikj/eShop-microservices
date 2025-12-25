namespace Catalog.Application.Checkout.Dtos;

public record CheckoutRequestDto(
	Guid CustomerId,
	List<CheckoutItemDto> Items
);
