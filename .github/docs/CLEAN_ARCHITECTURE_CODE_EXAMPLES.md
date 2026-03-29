# Clean Architecture - Code Examples from Your Project

**Real code from Orders, Payments, and CatalogService showing each layer in practice**

---

## ?? Complete Example: "Pay for Order" Request

Let's trace a real request through all 4 layers using your actual code.

### HTTP Request
```http
POST /api/orders/commands/{orderId}/pay
```

---

## Layer 1: ?? API LAYER - HTTP Entry Point

**File:** `Orders/Orders.Api/Controllers/OrdersCommandController.cs`

```csharp
[ApiController]
[Route("api/orders/commands")]
public class OrdersCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{orderId:guid}/pay")]
    public async Task<IActionResult> Pay(Guid orderId)
    {
        // Step 1: Convert HTTP request to command
        var command = new PayOrderCommand(orderId);

        // Step 2: Send to Application layer (MediatR handler)
        await _mediator.Send(command);

        // Step 3: Return HTTP response
        return NoContent();
    }
}
```

**What's happening:**
- ? Receives HTTP request
- ? Converts to command (PayOrderCommand)
- ? Delegates to Application layer via MediatR
- ? Returns HTTP response

**API Layer Responsibility:** "Accept HTTP, delegate to Application, return response"

---

## Layer 2: ?? APPLICATION LAYER - Orchestration

**File:** `Orders/Orders.Application/Ordering/Commands/PayOrder/PayOrderCommandHandler.cs`

```csharp
public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand>
{
    // Inject abstractions (interfaces)
    private readonly IOrderRepository _repo;        // ? Abstraction!
    private readonly IEventPublisher _publisher;    // ? Abstraction!

    public PayOrderCommandHandler(
        IOrderRepository repo,
        IEventPublisher publisher)
    {
        _repo = repo;
        _publisher = publisher;
    }

    public async Task Handle(
        PayOrderCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Get order (via abstraction - don't care HOW)
        var order = await _repo.GetByIdAsync(
            request.OrderId,
            cancellationToken)
            ?? throw new Exception("Order not found");

        // Step 2: Validate state (via domain logic)
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order is not payable");

        // Step 3: Publish event (via abstraction - don't care HOW)
        await _publisher.PublishAsync(
            new OrderPaymentRequested(
                order.Id,
                order.CustomerId,
                order.TotalPrice
            ));
    }
}
```

**What's happening:**
- ? Receives command from API layer
- ? Uses abstractions (IOrderRepository, IEventPublisher)
- ? Calls domain logic (Order state validation)
- ? Coordinates the flow
- ? Doesn't care HOW order is saved or event is published

**Application Layer Responsibility:** "Coordinate: fetch order ? validate ? publish event"

**Key Point:** This handler NEVER imports:
- ? EF Core (OrdersDbContext)
- ? HTTP libraries (ASP.NET Core)
- ? RabbitMQ libraries
- ? Only: Domain, DTOs, Abstractions

---

## Layer 3: ??? DOMAIN LAYER - Business Logic

**File:** `Orders/Orders.Domain/Entities/Order.cs`

```csharp
public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal TotalPrice { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }  // EF Core

    // Constructor with validation
    public Order(
        Guid orderId,
        Guid customerId,
        DateTime createdAt,
        decimal totalPrice)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId is required");
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required");
        if (totalPrice <= 0)
            throw new ArgumentException("TotalPrice must be greater than zero");

        Id = orderId;
        CustomerId = customerId;
        CreatedAt = createdAt;
        TotalPrice = totalPrice;
        Status = OrderStatus.Pending;  // ? Business rule!
    }

    // Business logic: Add item (only when pending)
    public void AddItem(
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                "Cannot modify order once processed");

        var item = new OrderItem(productId, productName, unitPrice, quantity);
        _items.Add(item);
    }

    // Business logic: Mark as paid (idempotent)
    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Paid)
            return;  // Idempotent - safe to call multiple times

        if (Status != OrderStatus.Pending)
            return;  // Graceful - ignore invalid transitions

        Status = OrderStatus.Paid;  // ? Change state
    }

    // Business logic: Cancel order
    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            return;  // Idempotent

        if (Status == OrderStatus.Shipped)
            return;  // Cannot cancel shipped

        if (Status == OrderStatus.Paid || Status == OrderStatus.Pending)
        {
            Status = OrderStatus.Cancelled;
        }
    }
}
```

**What's happening:**
- ? Pure business logic - no external dependencies!
- ? Validates state transitions
- ? Enforces business rules
- ? Idempotent methods (safe to call multiple times)

**Domain Layer Responsibility:** "Define business rules and validate state"

**Key Point:** This class has ZERO imports except:
- ? `using System;` (standard library only!)
- ? NO EF Core
- ? NO HTTP
- ? NO external services

**This is testable without any infrastructure!**

```csharp
// Test - no database needed!
[Test]
public void MarkAsPaid_WhenPending_ChangesStatus()
{
    var order = new Order(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 100m);
    order.MarkAsPaid();
    Assert.That(order.Status, Is.EqualTo(OrderStatus.Paid));
}
```

---

## Layer 4: ?? INFRASTRUCTURE LAYER - Implementation

### Part 4a: Repository Implementation

**File:** `Orders/Orders.Infrastructure/Repositories/OrderRepository.cs`

```csharp
public class OrderRepository : IOrderRepository  // ? Implements abstraction!
{
    private readonly OrdersDbContext _dbContext;  // ? EF Core

    public OrderRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Implement: Get order from database
    public async Task<Order?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)                  // ? EF Core query
            .FirstOrDefaultAsync(
                o => o.Id == orderId,
                cancellationToken);
    }

    // Implement: Save order to database
    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        _dbContext.Orders.Add(order);               // ? Add to EF Core
        await _dbContext.SaveChangesAsync(cancellationToken);  // ? Save to DB
    }

    // Implement: Update order in database
    public async Task UpdateAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        _dbContext.Orders.Update(order);            // ? Update in EF Core
        await _dbContext.SaveChangesAsync(cancellationToken);  // ? Save to DB
    }
}
```

**What's happening:**
- ? Implements `IOrderRepository` interface (defined in Application layer)
- ? Uses `OrdersDbContext` (EF Core - infrastructure detail!)
- ? Talks directly to database
- ? Handles SQL details

**Key Point:** Infrastructure imports and uses:
- ? EF Core (`DbContext`, `Include`, `FirstOrDefaultAsync`)
- ? Application layer abstractions (`IOrderRepository`)
- ? Domain entities (`Order`)

---

### Part 4b: Event Publisher Implementation

**File:** `Orders/Orders.Infrastructure/Messaging/MassTransitEventPublisher.cs`

```csharp
public class MassTransitEventPublisher : IEventPublisher  // ? Implements abstraction!
{
    private readonly OrdersDbContext _dbContext;  // ? EF Core

    public MassTransitEventPublisher(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Implement: Publish event (using Outbox pattern)
    public async Task PublishAsync<T>(T message) where T : class
    {
        // Create OutboxMessage (not published directly!)
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(T).AssemblyQualifiedName!,    // ? Store type name
            Content = JsonSerializer.Serialize(message)  // ? Store as JSON
        };

        // Save to database (Transactional Outbox pattern)
        _dbContext.OutboxMessages.Add(outboxMessage);
        await _dbContext.SaveChangesAsync();
    }
}
```

**What's happening:**
- ? Implements `IEventPublisher` interface
- ? Uses Outbox pattern (writes to database, not direct RabbitMQ)
- ? Ensures reliable delivery

**Why this matters:**
- Application layer uses `IEventPublisher` (abstraction)
- Infrastructure implements via Outbox pattern
- Could later swap to different pattern without changing Application!

---

### Part 4c: Background Service (Outbox Processor)

**File:** `Orders/Orders.Infrastructure/Messaging/OutboxProcessor.cs`

```csharp
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                // Log error
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessages(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrdersDbContext>();
        var publishEndpoint = scope.ServiceProvider
            .GetRequiredService<IPublishEndpoint>();

        // Get unprocessed messages
        var messages = await dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                // Deserialize and publish
                var type = Type.GetType(message.Type);
                var @event = JsonSerializer.Deserialize(message.Content, type);
                await publishEndpoint.Publish(@event, cancellationToken);

                // Mark as processed
                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

**What's happening:**
- ? Polls database for unprocessed events
- ? Publishes to RabbitMQ
- ? Marks as processed (idempotent)
- ? Ensures reliable delivery

**Infrastructure Layer Responsibility:** "Implement the abstractions with actual infrastructure"

---

## ?? Complete Request Journey Summary

```
REQUEST: POST /api/orders/commands/550e8400/pay

LAYER 1 - API (OrdersCommandController)
    ?? Receives HTTP request
    ?? Creates PayOrderCommand(550e8400)
    ?? Sends to MediatR

LAYER 2 - APPLICATION (PayOrderCommandHandler)
    ?? Receives command
    ?? Calls IOrderRepository.GetByIdAsync(550e8400)
       [Uses abstraction - doesn't know it's SQL Server!]
    ?? Gets Order entity back
    ?? Calls Order.MarkAsPaid() [Domain logic]
    ?? Calls IEventPublisher.PublishAsync(OrderPaymentRequested)
       [Uses abstraction - doesn't know it's Outbox pattern!]

LAYER 3 - DOMAIN (Order.MarkAsPaid)
    ?? Validates: Status == Pending? ?
    ?? Changes: Status = Paid
    ?? Returns to Application

LAYER 4 - INFRASTRUCTURE (OrderRepository + MassTransitEventPublisher)
    ?? OrderRepository.GetByIdAsync()
       ?? Uses OrdersDbContext (EF Core)
       ?? Queries SQL Server database
       ?? Returns Order entity
    ?? MassTransitEventPublisher.PublishAsync()
       ?? Creates OutboxMessage
       ?? Saves to SQL Server database
       ?? (Later: OutboxProcessor publishes to RabbitMQ)

RESPONSE: 204 NoContent
```

---

## ?? Why This Structure Matters

### Benefit 1: Easy to Test Domain Logic

```csharp
// ? Test domain without any infrastructure!
[TestFixture]
public class OrderTests
{
    [Test]
    public void MarkAsPaid_WhenPending_SuccessfullyTransitions()
    {
        // Arrange
        var order = new Order(
            Guid.NewGuid(),      // No database needed!
            Guid.NewGuid(),
            DateTime.UtcNow,
            100m
        );

        // Act
        order.MarkAsPaid();

        // Assert
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Paid));
    }

    [Test]
    public void MarkAsPaid_WhenAlreadyPaid_IsIdempotent()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 100m);
        order.MarkAsPaid();
        order.MarkAsPaid();  // Call twice
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Paid));  // Still Paid
    }
}
```

---

### Benefit 2: Easy to Test Application Logic with Mocks

```csharp
// ? Test orchestration with mocked infrastructure!
[TestFixture]
public class PayOrderCommandHandlerTests
{
    [Test]
    public async Task Handle_PublishesEvent()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var mockPublisher = new Mock<IEventPublisher>();
        var handler = new PayOrderCommandHandler(mockRepo.Object, mockPublisher.Object);

        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 100m);
        mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await handler.Handle(new PayOrderCommand(order.Id), CancellationToken.None);

        // Assert
        mockPublisher.Verify(x => x.PublishAsync(It.IsAny<OrderPaymentRequested>()), Times.Once);
    }
}
```

---

### Benefit 3: Easy to Swap Implementations

```csharp
// Current: SQL Server
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

// Want to switch to MongoDB?
// builder.Services.AddScoped<IOrderRepository, MongoOrderRepository>();

// Application layer doesn't change! ?
// Domain layer doesn't change! ?
// Only Infrastructure changes! ?
```

---

## ?? Quick Reference

### Which layer for each responsibility?

```
? "Save order to database"
   ? Infrastructure (OrderRepository)

? "Order state is invalid for payment"
   ? Domain (Order.MarkAsPaid validation)

? "Get order, validate, publish event"
   ? Application (PayOrderCommandHandler)

? "Handle HTTP /api/orders/pay"
   ? API (OrdersCommandController)
```

---

## ? Summary

```
API LAYER:
    OrdersCommandController
    ? delegates

APPLICATION LAYER:
    PayOrderCommandHandler
    ? uses

DOMAIN LAYER:
    Order.MarkAsPaid()
    ? ?

INFRASTRUCTURE LAYER:
    OrderRepository + MassTransitEventPublisher
    (implements application abstractions)
```

Each layer has ONE responsibility and ONE reason to change!

