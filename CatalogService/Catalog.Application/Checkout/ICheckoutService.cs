using Catalog.Application.Checkout.Dtos;

namespace Catalog.Application.Checkout;

public interface ICheckoutService
{
	Task CheckoutAsync(CheckoutRequestDto request);
}
