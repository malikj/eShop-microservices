using Microsoft.AspNetCore.Mvc;
using Orders.Application.Abstractions.Repositories;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersQueryController : ControllerBase
{
    private readonly IOrderReadRepository _readRepository;

    public OrdersQueryController(IOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId)
    {
        if (customerId.HasValue)
            return Ok(await _readRepository.GetByCustomerIdAsync(customerId.Value));

        return Ok(await _readRepository.GetAllAsync());
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId)
    {
        var order = await _readRepository.GetByIdAsync(orderId);
        return order is null ? NotFound() : Ok(order);
    }
}

