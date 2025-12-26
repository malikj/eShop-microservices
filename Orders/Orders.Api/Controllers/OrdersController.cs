using Microsoft.AspNetCore.Mvc;
using Orders.Application.Abstractions.Repositories;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderReadRepository _readRepository;

    public OrdersController(IOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId)
    {
        if (customerId.HasValue)
        {
            var orders = await _readRepository
                .GetByCustomerIdAsync(customerId.Value);

            return Ok(orders);
        }

        var allOrders = await _readRepository.GetAllAsync();
        return Ok(allOrders);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId)
    {
        Console.WriteLine($"[OrdersQueryController] GetById hit: {orderId}");

        var order = await _readRepository.GetByIdAsync(orderId);

        if (order == null)
            return NotFound();

        return Ok(order);
    }
}
