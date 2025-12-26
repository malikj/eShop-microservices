//using Catalog.Api.Messaging;
//using Microsoft.AspNetCore.Mvc;

//namespace Catalog.Api.Controllers;

//[ApiController]
//[Route("api/events-test")]
//public class EventsTestController : ControllerBase
//{
//    private readonly OrderEventPublisher _publisher;

//    public EventsTestController(OrderEventPublisher publisher)
//    {
//        _publisher = publisher;
//    }

//    [HttpPost("publish-order")]
//    public async Task<IActionResult> PublishOrder()
//    {
//        await _publisher.PublishOrderRequestedAsync();
//        return Ok("OrderRequested event published");
//    }
//}
