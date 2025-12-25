
using Catalog.Application.Categories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Catalog.Application.Checkout;
using Catalog.Application.Checkout.Dtos;
using Microsoft.AspNetCore.Mvc;


namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    //[HttpPost]
    //public async Task<IActionResult> Checkout(
    //    [FromBody] CheckoutRequestDto request)
    //{
    //    await _checkoutService.CheckoutAsync(request);
    //    return Accepted(); // async processing
    //}

    [HttpPost]
    public async Task<IActionResult> Checkout(
    [FromBody] CheckoutRequestDto request)
    {
        var orderId = await _checkoutService.CheckoutAsync(request);
        return Accepted(new { orderId });
    }

}
