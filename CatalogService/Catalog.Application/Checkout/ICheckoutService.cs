using Catalog.Application.Checkout.Dtos;

namespace Catalog.Application.Checkout;

public interface ICheckoutService
{
    Task<Guid> CheckoutAsync(CheckoutRequestDto request);
}
