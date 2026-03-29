# Clean Architecture: Why We Separate API, Application, Domain, Infrastructure

**A Comprehensive Guide to Understanding the 4-Layer Architecture**

---

## ?? The Simple Answer

Each microservice is divided into 4 layers to achieve:
- **Testability** - Each layer can be tested independently
- **Maintainability** - Changes in one layer don't break others
- **Flexibility** - Easy to swap implementations
- **Clarity** - Clear responsibility for each layer

---

## ?? The Four Layers Explained

### Layer 1: ??? **DOMAIN** (The Heart)

**What It Is:**
The innermost layer containing **pure business logic** with **NO external dependencies**

**What Lives Here:**
- Entities (Order, OrderItem, Product, Category)
- Enums (OrderStatus)
- Repository Interfaces (IOrderRepository, IProductRepository)
- Business Rules & Validation

**The Rule:**
```
Domain layer NEVER imports from:
? Databases (EF Core, SQL Server)
? HTTP frameworks (ASP.NET Core)
? External services (RabbitMQ, APIs)
? Only imports: C# standard library
```

**Why Separate:**
Domain is the **pure business logic** - the intellectual property of your business. It should be:
- Testable without infrastructure
- Reusable in any context (console app, API, background service)
- Independent of technology choices

**Example from OrdersService:**

```csharp
// ? GOOD - Domain layer (pure business logic)
Orders/Orders.Domain/Entities/Order.cs
```

```csharp
public class Order
{
    public Guid Id { get; private set; }
    public OrderStatus Status { get; private set; }

    // Business rule: Cannot mark as paid twice
    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Paid)
            return;  // Idempotent - safe to call multiple times

        if (Status != OrderStatus.Pending)
            return;  // Ignore invalid transitions

        Status = OrderStatus.Paid;  // ? Business logic!
    }
}
```

**No imports from:** EF Core, HTTP, databases, external APIs

---

### Layer 2: ?? **APPLICATION** (The Orchestrator)

**What It Is:**
The **use case layer** that orchestrates domain logic and external services

**What Lives Here:**
- Command Handlers (CreateOrderCommandHandler, PayOrderCommandHandler)
- Query Handlers (get orders by ID, list all orders)
- DTOs (Data Transfer Objects for requests/responses)
- Service Abstractions (IEventPublisher, IOrderRepository)
- Business Workflow Logic

**The Rule:**
```
Application layer:
? Uses Domain layer
? Calls repository abstractions (interfaces)
? Publishes events through abstractions
? Does NOT reference Infrastructure directly
? Does NOT reference API layer
```

**Why Separate:**
Application layer is the **"what to do"** - it defines the use cases but doesn't care HOW they're done.

Examples:
- "Create an order" - coordination of: fetch products, create Order entity, save to repo, publish event
- "Pay for order" - coordination of: fetch order, validate state, publish payment request
- But HOW the order is saved? (SQL? MongoDB? File?) ? Don't care! Use repository interface

**Example from OrdersService:**

```csharp
// ? GOOD - Application layer (orchestration)
Orders/Orders.Application/Ordering/Commands/PayOrder/PayOrderCommandHandler.cs
```

```csharp
public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand>
{
    private readonly IOrderRepository _repo;           // ? Interface!
    private readonly IEventPublisher _publisher;       // ? Interface!

    public async Task Handle(PayOrderCommand request, CancellationToken ct)
    {
        // Step 1: Fetch order (via repository abstraction)
        var order = await _repo.GetByIdAsync(request.OrderId, ct);

        // Step 2: Validate state (domain logic)
        if (order?.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order not payable");

        // Step 3: Publish event (via abstraction)
        await _publisher.PublishAsync(
            new OrderPaymentRequested(order.Id, order.CustomerId, order.TotalPrice)
        );
    }
}
```

**Notice:**
- Uses `IOrderRepository` (abstraction) - doesn't know if it's SQL, Mongo, File
- Uses `IEventPublisher` (abstraction) - doesn't know if it's RabbitMQ, Azure Service Bus, etc.
- Pure orchestration - "IF order is pending, THEN publish payment request"

---

### Layer 3: ?? **INFRASTRUCTURE** (The Implementer)

**What It Is:**
The **implementation details layer** - actual database code, messaging, external services

**What Lives Here:**
- DbContext (EF Core database configuration)
- Repository Implementations (OrderRepository implementing IOrderRepository)
- Event Publisher Implementations (MassTransitEventPublisher implementing IEventPublisher)
- Message Consumers (OrderRequestedConsumer, PaymentSucceededConsumer)
- Database Migrations
- Outbox/Inbox tables and logic

**The Rule:**
```
Infrastructure layer:
? References Application layer (for interfaces)
? References Domain layer (for entities)
? Implements the abstraction interfaces
? Contains all external dependencies (EF Core, RabbitMQ, etc.)
```

**Why Separate:**
Infrastructure is **"HOW to do it"** - all the gritty implementation details.

This separation allows:
- Swapping databases: Change from SQL Server ? MongoDB (only change Infrastructure)
- Swapping message broker: Change from RabbitMQ ? Azure Service Bus (only change Infrastructure)
- Testing without infrastructure: Unit tests can mock the interfaces

**Example from OrdersService:**

```csharp
// ? GOOD - Infrastructure layer (implementation)
Orders/Orders.Infrastructure/Repositories/OrderRepository.cs
```

```csharp
public class OrderRepository : IOrderRepository  // ? Implements abstraction
{
    private readonly OrdersDbContext _dbContext;  // ? EF Core (infrastructure detail!)

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        _dbContext.Orders.Add(order);  // ? Direct database interaction
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }
}
```

**Notice:**
- Uses `OrdersDbContext` (EF Core - infrastructure detail!)
- Direct database operations (Add, SaveChanges)
- Implements `IOrderRepository` interface defined in Application layer

---

### Layer 4: ?? **API** (The Entry Point)

**What It Is:**
The **HTTP entry point** - controllers, middleware, startup configuration

**What Lives Here:**
- Controllers (OrdersCommandController, OrdersQueryController)
- HTTP Middleware (CorrelationIdMiddleware)
- Event Consumers (MessageHandlers: OrderRequestedConsumer, PaymentSucceededConsumer)
- Startup/Program Configuration (DI setup, service registration)
- Logging & Observability setup

**The Rule:**
```
API layer:
? References Application layer (to send commands/queries)
? Registers dependencies (DI setup)
? Handles HTTP requests/responses
? Implements event consumers
```

**Why Separate:**
API layer is the **"where requests come in"** - it's the delivery mechanism.

This separation allows:
- Reuse same Application logic from multiple entry points (HTTP API, gRPC, background jobs)
- Easy to test without HTTP
- Easy to switch from HTTP ? gRPC without changing business logic

**Example from OrdersService:**

```csharp
// ? GOOD - API layer (entry point)
Orders/Orders.Api/Controllers/OrdersCommandController.cs
```

```csharp
[ApiController]
[Route("api/orders/commands")]
public class OrdersCommandController : ControllerBase
{
    private readonly IMediator _mediator;  // ? From Application layer

    [HttpPost("{orderId:guid}/pay")]
    public async Task<IActionResult> Pay(Guid orderId)
    {
        // Accept HTTP request
        // Send to Application layer
        await _mediator.Send(new PayOrderCommand(orderId));
        // Return HTTP response
        return NoContent();
    }
}
```

**Notice:**
- Pure HTTP handling - translates HTTP request to PayOrderCommand
- Delegates to Application layer (IMediator)
- Returns HTTP response

---

## ?? How They Work Together

### The Request Journey

```
???????????????????????????????????????????????????????????????
? 1. HTTP Request Arrives                                     ?
?    POST /api/orders/commands/{orderId}/pay                  ?
???????????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????????
? 2. API LAYER (Entry Point)                                  ?
?    OrdersCommandController.Pay()                            ?
?    ?? Receives HTTP request                                 ?
?    ?? Converts to PayOrderCommand                           ?
?    ?? Sends to Application layer                            ?
???????????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????????
? 3. APPLICATION LAYER (Orchestration)                        ?
?    PayOrderCommandHandler.Handle()                          ?
?    ?? Calls IOrderRepository.GetByIdAsync() [abstraction]   ?
?    ?? Calls Order.MarkAsPaid() [domain logic]               ?
?    ?? Calls IEventPublisher.PublishAsync() [abstraction]    ?
???????????????????????????????????????????????????????????????
             ?                               ?
???????????????????????????   ????????????????????????????????
? 4a. INFRASTRUCTURE      ?   ? 4b. INFRASTRUCTURE          ?
?     (Getting Order)     ?   ?     (Publishing Event)       ?
?                         ?   ?                              ?
? OrderRepository         ?   ? MassTransitEventPublisher    ?
? ?? OrdersDbContext      ?   ? ?? Creates OutboxMessage     ?
?    ?? EF Core query     ?   ?    ?? Saves to database      ?
?       to SQL Server     ?   ?       ?? SQL Server          ?
???????????????????????????   ????????????????????????????????
           ?                               ?
???????????????????????????????????????????????????????????????
? 5. DOMAIN LAYER (Business Logic)                            ?
?    Order.MarkAsPaid()                                       ?
?    ?? Validates state: Pending ? Paid                       ?
?    ?? Applies business rule                                 ?
???????????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????????
? 6. HTTP Response                                            ?
?    204 NoContent                                            ?
?    ? Order paid, event published                            ?
???????????????????????????????????????????????????????????????
```

---

## ?? Why This Matters - The Real Benefits

### Benefit 1: **Testability**

```csharp
// ? Easy to test: No database needed!
[Test]
public void Order_MarkAsPaid_WhenPending_ChangesStatus()
{
    // Arrange - Create just a domain entity (no infrastructure!)
    var order = new Order(
        orderId: Guid.NewGuid(),
        customerId: Guid.NewGuid(),
        createdAt: DateTime.UtcNow,
        totalPrice: 100m
    );

    // Act
    order.MarkAsPaid();

    // Assert
    Assert.That(order.Status, Is.EqualTo(OrderStatus.Paid));
}
```

**Why this matters:**
- No need for database setup
- No need for mock RabbitMQ
- Test runs in milliseconds
- 100% confidence in business logic

---

### Benefit 2: **Flexibility - Swap Implementations**

**Scenario: We want to use MongoDB instead of SQL Server**

```
BEFORE (Without clean architecture):
? Database code everywhere in the codebase
? Need to change multiple files
? Risky refactoring

AFTER (With clean architecture):
? Create new MongoDB repository class
? Update DI registration in API layer
? Everything else unchanged!
```

```csharp
// Old: SQL Server
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// New: MongoDB  
builder.Services.AddScoped<IOrderRepository, MongoOrderRepository>();
// ? Same interface, different implementation!
```

---

### Benefit 3: **Reusability - Use Business Logic Anywhere**

```csharp
// Same business logic works in multiple contexts:

// Context 1: HTTP API
public class OrdersCommandController
{
    public async Task<IActionResult> Pay(Guid orderId)
    {
        await _mediator.Send(new PayOrderCommand(orderId));
        return NoContent();
    }
}

// Context 2: Background Job
public class PaymentConfirmationBackgroundJob
{
    public async Task ExecuteAsync(string orderId)
    {
        await _mediator.Send(new PayOrderCommand(Guid.Parse(orderId)));
    }
}

// Context 3: gRPC Service
public override async Task Pay(PayRequest request, ServerCallContext context)
{
    await _mediator.Send(new PayOrderCommand(Guid.Parse(request.OrderId)));
    return new PayResponse();
}
```

**Same PayOrderCommandHandler used everywhere!**

---

### Benefit 4: **Maintainability - Clear Responsibilities**

```
When database performance is slow:
? Look in Infrastructure layer (OrdersDbContext, queries)

When business rule is wrong:
? Look in Domain layer (Order.cs)

When API response format is wrong:
? Look in API layer (Controller)

When coordination logic is buggy:
? Look in Application layer (Handler)
```

**Each layer has ONE clear responsibility:**
- Domain: Business logic
- Application: Coordination
- Infrastructure: Implementation details
- API: HTTP handling

---

## ?? Visual Layer Dependency

```
???????????????????????????????????????????
?          API LAYER                      ?
?  (HTTP Controllers, Consumers)          ?
?  ? depends on                           ?
???????????????????????????????????????????
?       APPLICATION LAYER                 ?
?  (Handlers, Interfaces)                 ?
?  ? depends on                           ?
???????????????????????????????????????????
?        DOMAIN LAYER                     ?
?  (Entities, Business Logic)             ?
?  ? depends on                           ?
???????????????????????????????????????????
?  C# Standard Library ONLY                ?
?  (No external dependencies)             ?
???????????????????????????????????????????

        ? implements ?

???????????????????????????????????????????
?     INFRASTRUCTURE LAYER                ?
?  (Repositories, DbContext, Consumers)   ?
?  ? implements interfaces from ?         ?
???????????????????????????????????????????

RULE: 
? Inner layers can be used independently
? Outer layers depend on inner layers
? Inner layers NEVER depend on outer layers
```

---

## ?? Real Examples from Your Code

### Example 1: Order Entity (Domain Layer)

```csharp
// Orders/Orders.Domain/Entities/Order.cs

public class Order
{
    public Guid Id { get; private set; }
    public OrderStatus Status { get; private set; }

    // Pure business logic - no database, no HTTP, no external dependencies!
    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Paid) return;
        if (Status != OrderStatus.Pending) return;
        Status = OrderStatus.Paid;
    }
}
```

? **This is pure business logic**
- No imports: using System; only!
- No dependencies: Just C# classes
- Testable: No mocking needed
- Reusable: Can use anywhere

---

### Example 2: Repository Interface (Application Layer)

```csharp
// Orders/Orders.Application/Abstractions/Repositories/IOrderRepository.cs

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task UpdateAsync(Order order, CancellationToken cancellationToken);
}
```

? **This is the abstraction**
- Defines WHAT to do (get order, save order)
- Doesn't say HOW (SQL, Mongo, File?)
- Application layer uses this
- Multiple implementations possible

---

### Example 3: Repository Implementation (Infrastructure Layer)

```csharp
// Orders/Orders.Infrastructure/Repositories/OrderRepository.cs

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _dbContext;  // ? EF Core!

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        _dbContext.Orders.Add(order);           // ? SQL Server implementation!
        await _dbContext.SaveChangesAsync(ct);
    }
}
```

? **This is the implementation**
- Uses EF Core (infrastructure detail)
- Talks to SQL Server (infrastructure detail)
- Implements the interface defined in Application layer
- Can be replaced with MongoDB version without changing anything else

---

### Example 4: Command Handler (Application Layer)

```csharp
// Orders/Orders.Application/Ordering/Commands/PayOrder/PayOrderCommandHandler.cs

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand>
{
    private readonly IOrderRepository _repo;        // ? Uses abstraction
    private readonly IEventPublisher _publisher;    // ? Uses abstraction

    public async Task Handle(PayOrderCommand request, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(request.OrderId, ct);  // ? Get via interface

        if (order?.Status != OrderStatus.Pending)
            throw new InvalidOperationException();

        await _publisher.PublishAsync(                              // ? Publish via interface
            new OrderPaymentRequested(order.Id, order.CustomerId, order.TotalPrice)
        );
    }
}
```

? **This is pure coordination**
- Uses abstractions (IOrderRepository, IEventPublisher)
- Doesn't care HOW order is saved or event is published
- Can be tested with mocks
- Reusable in any context

---

### Example 5: Controller (API Layer)

```csharp
// Orders/Orders.Api/Controllers/OrdersCommandController.cs

[ApiController]
[Route("api/orders/commands")]
public class OrdersCommandController : ControllerBase
{
    private readonly IMediator _mediator;  // ? From MediatR (Application)

    [HttpPost("{orderId:guid}/pay")]
    public async Task<IActionResult> Pay(Guid orderId)
    {
        await _mediator.Send(new PayOrderCommand(orderId));
        return NoContent();
    }
}
```

? **This is pure HTTP handling**
- Receives HTTP request
- Sends to Application layer (MediatR)
- Returns HTTP response
- Easy to test without HTTP

---

## ?? Junior Guidance - Simple Analogy

### Think of it like a Restaurant ??

```
???????????????????????????????????????????????
?  ?? API LAYER - The Front Desk             ?
?  (Customer walks in, places order)          ?
?  • Receives customer order                  ?
?  • Writes down what they want               ?
?  • Hands to kitchen                         ?
???????????????????????????????????????????????
             ?
???????????????????????????????????????????????
?  ????? APPLICATION LAYER - The Manager       ?
?  (Coordinates between front & kitchen)      ?
?  • Reads customer order                     ?
?  • Checks: "Can we make this?"              ?
?  • Tells chef: "Make 2 pizzas"              ?
?  • Tells server: "Set up table"             ?
?  • Coordinates everything                  ?
???????????????????????????????????????????????
         ?                     ?
????????????????????  ????????????????????
? ????? KITCHEN       ?  ? ????? STAFF       ?
? (Makes pizza)   ?  ? (Sets tables)    ?
? Infrastructure  ?  ? Infrastructure   ?
????????????????????  ????????????????????
         ?                     ?
         ???????????????????????
                  ?
???????????????????????????????????????????????
?  ?? DOMAIN LAYER - The Recipe               ?
?  "Make a good pizza: good dough, sauce, etc"?
?  (The business rules of good pizza!)        ?
???????????????????????????????????????????????
```

**Why separate?**
- **Domain (Recipe):** Never changes based on kitchen equipment
- **Application (Manager):** Coordinates between front & back
- **Infrastructure (Kitchen):** Could use different ovens, suppliers
- **API (Front Desk):** Could have different customer interfaces (dine-in, delivery, takeout)

**Changes needed:**
- Better recipe? ? Modify Domain
- Different delivery method? ? Add new API (could use same Application logic)
- Buy new oven? ? Change Infrastructure only
- Need more coordination steps? ? Update Application

---

## ? Summary: Why Each Layer Exists

| Layer | Why | What Happens If Missing |
|-------|-----|------------------------|
| **Domain** | Pure business logic | Business rules scattered everywhere; untestable |
| **Application** | Orchestration/Use cases | No clear coordination; duplicate logic |
| **Infrastructure** | Implementation details | Business logic coupled to database; can't swap databases |
| **API** | HTTP entry point | Can't reuse logic for gRPC, background jobs, etc. |

---

## ?? Remember

```
DOMAIN Layer:
  "What is a valid order?" (Business rules)

APPLICATION Layer:
  "How do we process a payment?" (Coordination)

INFRASTRUCTURE Layer:
  "How do we save to SQL Server?" (Implementation)

API Layer:
  "How do we receive HTTP requests?" (Entry point)
```

Each layer answers ONE question and doesn't care about the others!

