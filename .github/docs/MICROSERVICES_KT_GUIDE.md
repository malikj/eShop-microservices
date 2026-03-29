# eShop Microservices - Complete Knowledge Transfer Guide

**Last Updated:** January 2025  
**Audience:** New team members, junior developers  
**Purpose:** Comprehensive reference for understanding the eShop microservices architecture

---

## 📚 Table of Contents

1. [Quick Start Overview](#quick-start-overview)
2. [Architecture at a Glance](#architecture-at-a-glance)
3. [The Three Microservices](#the-three-microservices)
4. [Complete User Journey](#complete-user-journey)
5. [Core Patterns](#core-patterns)
6. [Data & Logic Flow](#data--logic-flow)
7. [Event Contracts](#event-contracts)
8. [Configuration & Startup](#configuration--startup)
9. [Common Tasks](#common-tasks)
10. [Troubleshooting](#troubleshooting)
11. [Key Takeaways](#key-takeaways)

---

## Quick Start Overview

### The System in 30 Seconds

eShop is a **distributed, event-driven microservices system** where:

- **CatalogService** manages products and initiates orders
- **OrdersService** orchestrates the order lifecycle
- **PaymentsService** validates and processes payments
- **RabbitMQ** connects everything asynchronously via events
- **SQL Server** persists data

**Philosophy:** Services are independent, loosely coupled, and communicate only through events.

### Core Technologies

```
🏗️  Architecture:    Clean Architecture + CQRS + Event-Driven
🗄️  Database:        SQL Server 2022
📨  Message Bus:     RabbitMQ 3.12
🔗  Orchestration:   MassTransit (consumer framework)
⚙️  Patterns:        Outbox, Inbox, State Machine, Repository
📊  Tracing:         OpenTelemetry + Serilog
```

---

## Architecture at a Glance

### High-Level System Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                       eShop Ecosystem                            │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────┐  ┌───────────────┐  ┌──────────────┐        │
│  │  CatalogService│  │ OrdersService │  │PaymentsService        │
│  │                │  │               │  │              │        │
│  │ • Products     │  │ • Orders      │  │ • Validation │        │
│  │ • Categories   │  │ • State mgmt  │  │ • Processing │        │
│  │ • Checkout     │  │ • Outbox/Inbox               │        │
│  └────────────────┘  └───────────────┘  └──────────────┘        │
│         │                     ↑ │                  ↑             │
│         │ OrderRequested      │ │ OrderPaymentReq  │             │
│         └──────→ RabbitMQ ←───┘ │                  │             │
│                                │                   │             │
│                 PaymentSucceeded/Failed             │             │
│                                │                   │             │
│                    ┌───────────┴───────────┐        │             │
│                    ↓                       ↓        │             │
│                    └──────→ (Back to Payments) ←──┘             │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ eShop.Contracts (Shared Event Schemas)                    │  │
│  │ • OrderRequested                                          │  │
│  │ • OrderPaymentRequested, PaymentSucceeded, PaymentFailed  │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

## The Three Microservices

### 1️⃣ CatalogService - Product Management & Sales Trigger

**Role:** Manages the product catalog and initiates the order lifecycle

**Responsibility:** 
- Store and retrieve products/categories
- Execute checkout logic
- Publish `OrderRequested` event

**Data Ownership:**
- Products (ID, Name, Price, CategoryId)
- Categories (ID, Name, Description)

**Complexity:** Medium

**Key Technologies:**
- ASP.NET Core REST API
- Entity Framework Core
- SQL Server (CatalogDb)
- MassTransit (event publishing)

#### Project Structure
```
CatalogService/
├── Catalog.Domain/
│   └── Entities: Product.cs, Category.cs
├── Catalog.Application/
│   ├── Products/ (ProductService, ProductDto, CreateProductCommand)
│   ├── Categories/ (CategoryService, CategoryDto)
│   └── Checkout/ (CheckoutService, CheckoutRequestDto)
├── Catalog.Infrastructure/
│   ├── Persistence/ (CatalogDbContext)
│   ├── Repositories/ (ProductRepository, CategoryRepository)
│   └── Messaging/ (MassTransitEventPublisher)
└── Catalog.Api/
    ├── Program.cs (startup configuration)
    ├── Controllers/ (ProductsController, CategoriesController, CheckoutController)
    └── Middleware/ (ExceptionHandlingMiddleware)
```

#### Key Endpoints
```
GET  /api/products                    # List all products
POST /api/products                    # Create product
GET  /api/categories                  # List all categories
POST /api/categories                  # Create category
POST /api/checkout                    # TRIGGER: Initiate order (publishes OrderRequested)
```

#### Database Schema
```sql
Products:
  Id (Guid), Name, Description, Price, CategoryId, IsActive

Categories:
  Id (int), Name, Description, IsActive
```

---

### 2️⃣ OrdersService - Order Orchestration & State Management

**Role:** Central orchestrator of the order lifecycle

**Responsibility:**
- Receive order requests from CatalogService
- Maintain order state (Pending → Paid → Shipped → Cancelled)
- Coordinate with PaymentsService
- Handle idempotent message processing

**Data Ownership:**
- Orders (ID, CustomerId, Status, TotalPrice, CreatedAt)
- OrderItems (ProductId, ProductName, UnitPrice, Quantity)
- OutboxMessages (events to publish)
- InboxMessages (processed events - deduplication)

**Complexity:** High

**Key Technologies:**
- ASP.NET Core REST API
- MediatR (command/query pattern)
- Entity Framework Core
- SQL Server (OrdersDb)
- MassTransit (consumers & event bus)
- Serilog (structured logging)
- OpenTelemetry (distributed tracing)

#### Project Structure
```
Orders/
├── Orders.Domain/
│   ├── Entities/ (Order.cs, OrderItem.cs)
│   └── Enums/ (OrderStatus.cs: Pending=1, Paid=2, Shipped=3, Cancelled=4)
├── Orders.Application/
│   ├── Ordering/
│   │   ├── Commands/ (CreateOrderCommand, PayOrderCommand, CancelOrderCommand + Handlers)
│   │   ├── Queries/ (OrderReadDto, OrderItemReadDto)
│   │   └── Dtos/ (CreateOrderItemDto)
│   └── Abstractions/ (IOrderRepository, IOrderReadRepository, IInboxRepository, IEventPublisher, ICorrelationIdAccessor)
├── Orders.Infrastructure/
│   ├── Persistence/ (OrdersDbContext, OutboxMessage, InboxMessage, Configurations)
│   ├── Repositories/ (OrderRepository, OrderReadRepository, InboxRepository)
│   ├── Messaging/ (MassTransitEventPublisher, OutboxProcessor BackgroundService)
│   └── Correlation/ (HttpCorrelationIdAccessor)
└── Orders.Api/
    ├── Program.cs (DI configuration, MassTransit setup)
    ├── Controllers/ (OrdersCommandController, OrdersQueryController)
    ├── Consumers/ (OrderRequestedConsumer, PaymentSucceededConsumer, PaymentFailedConsumer)
    ├── Middleware/ (CorrelationIdMiddleware)
    └── BackgroundServices/ (OutboxProcessor runs here)
```

#### Key Endpoints
```
# Queries (Read-only)
GET  /api/orders                      # List all orders
GET  /api/orders?customerId={id}      # List customer's orders
GET  /api/orders/{orderId}            # Get single order details

# Commands (State-changing)
POST /api/orders/commands/{orderId}/pay     # Initiate payment
POST /api/orders/commands/{orderId}/cancel  # Cancel order
```

#### Database Schema
```sql
Orders:
  Id (Guid), CustomerId (Guid), Status (int), TotalPrice (decimal), CreatedAt (DateTime)

OrderItems:
  ProductId (Guid), ProductName (string), UnitPrice (decimal), Quantity (int), OrderId (Guid FK)

OutboxMessages:
  Id (Guid), Type (string), Content (JSON), OccurredOnUtc, ProcessedOnUtc (nullable), Error (nullable)

InboxMessages:
  MessageId (Guid), Consumer (string), CorrelationId (Guid), ProcessedAt (DateTime)
```

#### State Machine
```
        ┌─────────┐
        │ PENDING │ ← Created by OrderRequested event
        └────┬────┘
             │
    [PaymentSucceeded]
             │
        ┌────▼────┐
        │   PAID   │ ← Ready for fulfillment
        └──────────┘

        ┌─────────┐
        │ PENDING │ ← Created by OrderRequested event
        └────┬────┘
             │
    [PaymentFailed]
             │
       ┌─────▼─────┐
       │ CANCELLED │ ← Payment rejected
       └───────────┘
```

#### Order Status Enum
```csharp
public enum OrderStatus
{
    Pending = 1,
    Paid = 2,
    Shipped = 3,
    Cancelled = 4
}
```

---

### 3️⃣ PaymentsService - Payment Processing & Validation

**Role:** Lightweight validator for payment requests

**Responsibility:**
- Receive payment requests
- Validate payment amount against business rules
- Publish payment success/failure events
- No database persistence (stateless)

**Data Ownership:**
- Payment validation logic only
- Payment entity (not persisted)

**Complexity:** Low

**Key Technologies:**
- ASP.NET Core minimal API
- MassTransit (consumer framework)
- RabbitMQ connection
- Serilog (structured logging)
- OpenTelemetry (distributed tracing)

#### Project Structure
```
Payments/
├── Payments.Domain/
│   └── Entities/ (Payment.cs - for future use)
├── Payments.Application/
│   └── (Currently sparse - ready for expansion)
├── Payments.Infrastructure/
│   └── (Placeholder - no DB yet)
└── Payments.Api/
    ├── Program.cs (startup configuration)
    ├── Consumers/ (OrderPaymentRequestedConsumer)
    └── appsettings.json (RabbitMQ config)
```

#### Key Logic
```csharp
// Business Rule: Accept payments up to $100,000 (100_0000 cents)
var success = message.Amount <= 100_0000;

if (success)
    PublishEndpoint.Publish(new PaymentSucceeded(orderId, DateTime.UtcNow));
else
    PublishEndpoint.Publish(new PaymentFailed(orderId, "Amount exceeds limit"));
```

---

### 4️⃣ eShop.Contracts - Shared Event Schemas

**Role:** The shared vocabulary for all inter-service communication

**Responsibility:**
- Define event types (records)
- Ensure type-safe contracts
- Version event schemas

**Complexity:** Minimal (no logic)

**Key Technologies:**
- C# Record types
- No dependencies (ultra-lightweight)

#### Events Defined
```csharp
// FROM CatalogService (Orders consumes)
OrderRequested(OrderId, CustomerId, CreatedAt, TotalAmount, Items[])

// FROM OrdersService (Payments consumes)
OrderPaymentRequested(OrderId, CustomerId, Amount)

// FROM PaymentsService (Orders consumes)
PaymentSucceeded(OrderId, PaidAt)
PaymentFailed(OrderId, Reason)

// FUTURE (Orders publishes)
OrderPaid(OrderId, CustomerId, PaidAt, TotalPrice)
OrderCancelled(OrderId, CustomerId, CancelledAt, Reason)
```

---

## Complete User Journey

### Timeline: From Checkout to Payment Confirmation

```
T+0ms
  └─ Customer clicks "Checkout" in CatalogService
     Request: POST /api/checkout { customerId, items[] }

T+10ms
  └─ CatalogService validates products
     └─ Calculates total price
     └─ Publishes OrderRequested to RabbitMQ

T+50ms
  └─ OrdersService OrderRequestedConsumer triggered
     └─ Converts to CreateOrderCommand
     └─ Sends to MediatR

T+60ms
  └─ CreateOrderCommandHandler executes
     └─ Creates Order aggregate (Status = Pending)
     └─ Adds OrderItems
     └─ Saves to OrdersDb
     └─ Returns OrderId

T+100ms
  └─ Customer sees order (GET /api/orders/{orderId})
     Response: { Status: "Pending", Items[], Total }

T+5000ms
  └─ Customer clicks "Pay"
     Request: POST /api/orders/commands/{orderId}/pay

T+5010ms
  └─ PayOrderCommandHandler executes
     └─ Publishes OrderPaymentRequested to Outbox
     └─ Saves OutboxMessage to OrdersDb

T+5030ms
  └─ OutboxProcessor (BackgroundService) picks up message
     └─ Publishes to RabbitMQ
     └─ Updates OutboxMessage.ProcessedOnUtc

T+5080ms
  └─ PaymentsService OrderPaymentRequestedConsumer triggered
     └─ Validates: Amount <= $100,000? YES ✓
     └─ Publishes PaymentSucceeded to RabbitMQ

T+5130ms
  └─ OrdersService PaymentSucceededConsumer triggered
     └─ Checks Inbox (idempotency)
     └─ Updates Order.Status = Paid
     └─ Records in InboxMessages

T+5200ms
  └─ Customer checks order (GET /api/orders/{orderId})
     Response: { Status: "Paid", Items[], Total }
     ✓ Order ready for fulfillment
```

---

## Core Patterns

### Pattern 1: Transactional Outbox ⭐

**Problem:** How to guarantee event delivery if service crashes?

**Solution:** Write events to database before publishing to message bus

```
HANDLER EXECUTION:
  1. Create OutboxMessage object
  2. _publisher.PublishAsync(event)
     └─ Writes OutboxMessage to _dbContext
  3. await _dbContext.SaveChangesAsync()
     └─ ONE ATOMIC TRANSACTION
     └─ Either both succeed or both fail

OUTBOX PROCESSOR (BackgroundService):
  Every 5 seconds:
    1. SELECT * FROM OutboxMessages WHERE ProcessedOnUtc IS NULL
    2. For each message:
       ├─ Deserialize JSON to event type
       ├─ IPublishEndpoint.Publish(event)
       └─ UPDATE ProcessedOnUtc = now
    3. Repeat

GUARANTEE: "At least once" delivery
EVEN IF: Service crashes, broker restarts, network fails
BECAUSE: Message is safely in database, will be retried
```

**When to Use:** Any event that must reach downstream consumers

---

### Pattern 2: Inbox Deduplication ⭐

**Problem:** What if message is delivered twice?

**Solution:** Track processed messages and skip duplicates

```
CONSUMER EXECUTION:
  1. Extract MessageId from RabbitMQ context
  2. Check InboxMessages table:
     SELECT * FROM InboxMessages 
     WHERE MessageId = 'msg-123' AND Consumer = 'PaymentSucceededConsumer'
  3. If NOT FOUND:
     ├─ Process the event (update Order.Status)
     ├─ INSERT INTO InboxMessages
     └─ SaveChangesAsync() [ATOMIC]
  4. If FOUND:
     └─ Return immediately (already processed)

GUARANTEE: "Exactly once" semantics
PROTECTS: Against RabbitMQ retries, network glitches
IDEMPOTENT: Safe to call same handler multiple times
```

**When to Use:** Any consumer that processes events (especially state changes)

---

### Pattern 3: CQRS - Command Query Responsibility Segregation

**Problem:** Read and write operations have different optimization needs

**Solution:** Separate into different code paths

```
WRITE SIDE (Commands):
  └─ IOrderRepository
     ├─ AddAsync(order)
     └─ UpdateAsync(order)
  └─ Used by: Handlers that create/modify orders
  └─ Behavior: With change tracking (can modify)

READ SIDE (Queries):
  └─ IOrderReadRepository
     ├─ GetAllAsync()
     ├─ GetByIdAsync(orderId)
     └─ GetByCustomerIdAsync(customerId)
  └─ Used by: HTTP GET endpoints returning DTOs
  └─ Behavior: AsNoTracking() (no modification)

BENEFIT:
  • Reads can use different optimization (indexes, caching)
  • Writes can ensure consistency
  • Can eventually scale to separate databases
```

**When to Use:** When read and write patterns diverge

---

### Pattern 4: State Machine

**Problem:** Orders have complex lifecycle with valid/invalid transitions

**Solution:** Enforce valid state transitions in domain entity

```csharp
public class Order
{
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    // Only valid from Pending state
    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Paid) return;  // Idempotent
        if (Status != OrderStatus.Pending) return;  // Graceful ignore
        Status = OrderStatus.Paid;
    }

    // Valid from Pending or Paid
    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled) return;  // Idempotent
        if (Status == OrderStatus.Shipped) return;  // Cannot cancel shipped
        if (Status == OrderStatus.Paid || Status == OrderStatus.Pending)
            Status = OrderStatus.Cancelled;
    }
}
```

**When to Use:** Complex entities with state-dependent behavior

---

## Data & Logic Flow

### Complete Request-Response Cycle

#### Flow 1: Order Creation
```
Entry Point:
  POST /api/checkout (CatalogService)

Request Payload:
  {
    "customerId": "750e8400-...",
    "items": [
      { "productId": "111", "quantity": 1 },
      { "productId": "222", "quantity": 2 }
    ]
  }

Processing Steps:
  1. CheckoutController.Checkout()
  2. CheckoutService.CheckoutAsync()
  3. ProductRepository.GetByIdsAsync() [DB READ]
  4. Calculate TotalAmount = (price1 * qty1) + (price2 * qty2)
  5. Create OrderId (Guid)
  6. IEventPublisher.PublishAsync(OrderRequested)
  7. SaveChangesAsync() [DB WRITE to OrdersDb via MassTransit]

Response:
  202 Accepted
  {
    "orderId": "550e8400-e29b-41d4-a716-446655440000"
  }

Event Published:
  OrderRequested {
    OrderId: "550e8400-...",
    CustomerId: "750e8400-...",
    CreatedAt: "2024-01-25T14:32:10Z",
    TotalAmount: 7499m,
    Items: [
      {
        ProductId: "111",
        ProductName: "Widget",
        UnitPrice: 5000,
        Quantity: 1
      },
      {
        ProductId: "222",
        ProductName: "Gadget",
        UnitPrice: 2499,
        Quantity: 2
      }
    ]
  }
```

#### Flow 2: Order Payment
```
Entry Point:
  POST /api/orders/commands/{orderId}/pay (OrdersService)

Processing Steps:
  1. OrdersCommandController.Pay(orderId)
  2. MediatR sends PayOrderCommand(orderId)
  3. PayOrderCommandHandler.Handle()
     ├─ _repo.GetByIdAsync(orderId) [DB READ]
     ├─ Validate: order.Status == Pending
     └─ _publisher.PublishAsync(OrderPaymentRequested)
  4. MassTransitEventPublisher.PublishAsync()
     ├─ Create OutboxMessage
     ├─ _dbContext.OutboxMessages.Add()
     └─ SaveChangesAsync() [DB WRITE - ATOMIC]
  5. Return 204 NoContent

OutboxMessage Created:
  {
    Id: "abc-123",
    Type: "OrderPaymentRequested",
    Content: "{\"OrderId\":\"550e8400-...\",\"Amount\":7499}",
    OccurredOnUtc: "2024-01-25T14:32:15Z",
    ProcessedOnUtc: null  ← Not yet published
  }

Later (every 5 seconds):
  OutboxProcessor.ExecuteAsync()
    1. SELECT * FROM OutboxMessages WHERE ProcessedOnUtc IS NULL
    2. Deserialize to OrderPaymentRequested
    3. IPublishEndpoint.Publish(orderPaymentReq)
    4. UPDATE OutboxMessages SET ProcessedOnUtc = now
```

#### Flow 3: Payment Validation & Response
```
Entry Point:
  OrderPaymentRequested event from RabbitMQ

Consumer:
  OrderPaymentRequestedConsumer.Consume()

Processing Steps:
  1. Extract message: { OrderId, CustomerId, Amount }
  2. Validate business rule:
     var success = Amount <= 100_0000;  // $100,000 limit
  3. If success:
     └─ IPublishEndpoint.Publish(
          new PaymentSucceeded(orderId, DateTime.UtcNow)
        )
  4. If failure:
     └─ IPublishEndpoint.Publish(
          new PaymentFailed(orderId, "Amount exceeds limit")
        )

Events Published Back to RabbitMQ:
  PaymentSucceeded {
    OrderId: "550e8400-...",
    PaidAt: "2024-01-25T14:32:18Z"
  }

  OR

  PaymentFailed {
    OrderId: "550e8400-...",
    Reason: "Amount exceeds allowed limit"
  }
```

#### Flow 4: Order Status Update
```
Entry Point:
  PaymentSucceeded event from RabbitMQ

Consumer:
  PaymentSucceededConsumer.Consume()

Processing Steps:
  1. Extract MessageId from context
  2. Inbox Deduplication Check:
     SELECT * FROM InboxMessages
     WHERE MessageId = 'msg-123'
     AND Consumer = 'PaymentSucceededConsumer'

     Result: NOT FOUND (first time)

  3. Fetch Order:
     _orders.GetByIdAsync(orderId) [DB READ]

  4. Validate state:
     if (order?.Status != OrderStatus.Pending) return;

  5. Update order:
     order.MarkAsPaid()
     └─ Order.Status = OrderStatus.Paid

  6. Record message processing:
     INSERT INTO InboxMessages (
       MessageId, Consumer, CorrelationId, ProcessedAt
     )

  7. Persist changes:
     _orders.UpdateAsync(order)
     └─ SaveChangesAsync() [ATOMIC TRANSACTION]

Result in Database:
  Orders table:
    UPDATE Orders SET Status = 2 WHERE Id = '550e8400-...'

  InboxMessages table:
    INSERT INTO InboxMessages (msg-123, PaymentSucceededConsumer, ...)
```

---

## Event Contracts

### Event Glossary

| Event | Publisher | Consumer | Trigger | Purpose |
|-------|-----------|----------|---------|---------|
| **OrderRequested** | CatalogService | OrdersService | User checkout | Order creation |
| **OrderPaymentRequested** | OrdersService | PaymentsService | User payment request | Payment processing |
| **PaymentSucceeded** | PaymentsService | OrdersService | Amount validates | Mark order as paid |
| **PaymentFailed** | PaymentsService | OrdersService | Amount exceeds limit | Cancel order |
| **OrderPaid** | OrdersService | (Future) | Order marked paid | Fulfill order notification |
| **OrderCancelled** | OrdersService | (Future) | Payment fails | Inventory rollback |

### Event Payload Examples

#### OrderRequested
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "customerId": "750e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2024-01-25T14:32:10Z",
  "totalAmount": 9998,
  "items": [
    {
      "productId": "111",
      "productName": "Widget",
      "unitPrice": 5000,
      "quantity": 1
    }
  ]
}
```

#### OrderPaymentRequested
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "customerId": "750e8400-e29b-41d4-a716-446655440000",
  "amount": 9998
}
```

#### PaymentSucceeded
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "paidAt": "2024-01-25T14:32:18Z"
}
```

#### PaymentFailed
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "reason": "Amount exceeds allowed limit"
}
```

---

## Configuration & Startup

### Environment Setup

#### Prerequisites
```
• .NET 8.0 SDK
• SQL Server 2022
• Docker & Docker Compose (for local RabbitMQ)
• Visual Studio 2022 or VS Code
```

#### Connection Strings
```json
{
  "ConnectionStrings": {
    "CatalogDb": "Server=localhost;Database=CatalogDb;User Id=sa;Password=YourPassword123!;",
    "OrdersDb": "Server=localhost;Database=OrdersDb;User Id=sa;Password=YourPassword123!;"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Docker Compose Setup

```yaml
# Start all services
docker-compose up -d

# Services:
# • SQL Server: localhost:1433
# • RabbitMQ: localhost:5672 (AMQP), localhost:15672 (Web UI)
# • CatalogService: http://localhost:5000
# • OrdersService: http://localhost:5001
# • PaymentsService: http://localhost:5002
```

### Startup Configuration Order

```
1. SQL Server
   └─ Databases: CatalogDb, OrdersDb

2. RabbitMQ
   └─ Broker ready for connections

3. CatalogService
   └─ Depends on: SQL Server
   └─ Publishes: OrderRequested

4. OrdersService
   └─ Depends on: SQL Server, RabbitMQ
   └─ BackgroundService: OutboxProcessor starts
   └─ Consumers: Listen for OrderRequested, PaymentSucceeded, PaymentFailed

5. PaymentsService
   └─ Depends on: RabbitMQ
   └─ Consumer: Listens for OrderPaymentRequested
```

### Program.cs Configuration

#### Orders Service Example
```csharp
// 1. Database
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));

// 2. Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderReadRepository, OrderReadRepository>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();

// 3. MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderRequestedConsumer>();
    x.AddConsumer<PaymentSucceededConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.UseMessageRetry(r => r.Ignore<InvalidOperationException>());
        cfg.ConfigureEndpoints(context);
    });
});

// 4. MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
});

// 5. Outbox Processor
builder.Services.AddHostedService<OutboxProcessor>();

// 6. Observability
builder.Services.AddScoped<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();
builder.Host.UseSerilog();

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
app.MapControllers();
app.Run();
```

---

## Common Tasks

### Task 1: Add a New Product

**Location:** CatalogService

**Steps:**
1. POST to `/api/products`
   ```json
   {
     "name": "New Widget",
     "description": "Best widget ever",
     "price": 99.99,
     "categoryId": 1
   }
   ```

2. ProductsController routes to ProductService.CreateAsync()

3. ProductService validates input via FluentValidation

4. Creates Product entity and saves via ProductRepository

5. Returns ProductId to client

**Files Involved:**
- `Catalog.Api/Controllers/ProductsController.cs`
- `Catalog.Application/Products/ProductService.cs`
- `Catalog.Infrastructure/Repositories/ProductRepository.cs`
- `Catalog.Domain/Entities/Product.cs`

---

### Task 2: Debug an Order Not Transitioning to Paid

**Symptoms:** Order remains Pending after payment

**Diagnosis Steps:**

1. **Check Order Status**
   ```
   GET /api/orders/{orderId}
   Look for: Status field
   ```

2. **Check RabbitMQ**
   ```
   Visit: http://localhost:15672 (RabbitMQ Management UI)
   Check: Queue depths, unacked messages
   ```

3. **Check OutboxMessages**
   ```sql
   SELECT * FROM OutboxMessages 
   WHERE ProcessedOnUtc IS NULL;
   ```
   If messages here: OutboxProcessor not running or failed

4. **Check OrdersDb Logs**
   ```
   Look at: Application logs in Orders.Api
   Search for: "OutboxProcessor", "PaymentSucceededConsumer"
   ```

5. **Check Inbox Duplicates**
   ```sql
   SELECT * FROM InboxMessages 
   WHERE MessageId = '{messageId}';
   ```
   If found: Already processed (check Order.Status)

**Common Issues:**
- RabbitMQ connection: Check appsettings.json host/credentials
- OutboxProcessor not running: Check if AddHostedService<OutboxProcessor>() called
- Consumer not registered: Check MassTransit x.AddConsumer<>() calls
- Consumer exception: Check application logs for errors

---

### Task 3: Add Retry Logic to a Consumer

**Location:** Orders.Api/Consumers/PaymentSucceededConsumer.cs

**Before:**
```csharp
public async Task Consume(ConsumeContext<PaymentSucceeded> context)
{
    // ... processing
    await _orders.UpdateAsync(order);
}
```

**After (with retry):**
```csharp
public async Task Consume(ConsumeContext<PaymentSucceeded> context)
{
    try
    {
        // ... processing
        await _orders.UpdateAsync(order);
    }
    catch (DbUpdateConcurrencyException)
    {
        // Retry through MassTransit
        throw;
    }
    catch (Exception ex)
    {
        // Log and skip (don't retry transient vs permanent failures)
        Log.Error(ex, "Failed to process payment");
        // throw or return based on error type
    }
}
```

**Configure Global Retry (Program.cs):**
```csharp
cfg.UseMessageRetry(r =>
{
    r.Ignore<InvalidOperationException>();  // Don't retry business logic errors
    r.Ignore<OrderNotFoundException>();

    r.Incremental(
        retryLimit: 5,
        initialInterval: TimeSpan.FromSeconds(1),
        intervalIncrement: TimeSpan.FromSeconds(2));
});
```

---

### Task 4: Track Request Across Services (Correlation ID)

**Feature:** Correlation ID middleware automatically injected

**How It Works:**
1. HTTP request arrives at OrdersService
2. CorrelationIdMiddleware extracts/generates X-Correlation-Id header
3. Serilog logs include CorrelationId
4. When publishing events, CorrelationId travels in message context

**Viewing Trace:**
```
Order arrives: X-Correlation-Id: abc-123
  ├─ OrdersService logs: CorrelationId=abc-123
  ├─ Published to RabbitMQ with correlation context
  └─ PaymentsService receives: CorrelationId=abc-123
      └─ Payment logs: CorrelationId=abc-123
         └─ Published back with same ID
            └─ OrdersService payment consumer: CorrelationId=abc-123
```

**Check Logs:**
```
[14:32:10 INF] CorrelationId=abc-123 Processing order...
[14:32:11 INF] CorrelationId=abc-123 Published OrderPaymentRequested
[14:32:12 INF] CorrelationId=abc-123 Payment validated
[14:32:13 INF] CorrelationId=abc-123 Order marked as paid
```

---

## Troubleshooting

### Issue: "RabbitMQ connection refused"

**Cause:** RabbitMQ not running or wrong host

**Solution:**
```bash
# Start RabbitMQ
docker-compose up -d rabbitmq

# Verify
docker ps | grep rabbitmq

# Check connection
telnet localhost 5672
```

Check `appsettings.json`:
```json
"RabbitMQ": {
  "Host": "localhost",  // or "rabbitmq" if in Docker
  "Username": "guest",
  "Password": "guest"
}
```

---

### Issue: "OutboxMessages not being processed"

**Cause:** OutboxProcessor not running or database issue

**Check:**
1. Is OutboxProcessor registered?
   ```csharp
   builder.Services.AddHostedService<OutboxProcessor>();
   ```

2. Check logs for OutboxProcessor errors

3. Verify database connection

4. Check OutboxMessages table:
   ```sql
   SELECT COUNT(*) FROM OutboxMessages WHERE ProcessedOnUtc IS NULL;
   ```

**Solution:** Restart service or manually trigger OutboxProcessor

---

### Issue: "Order stuck in Pending state after payment"

**Cause:** PaymentSucceededConsumer not receiving message or failing

**Debug:**
1. Check RabbitMQ queues for undelivered messages
2. Check InboxMessages table (is payment already recorded?)
3. Check Order.Status in database directly
4. Look for consumer exceptions in logs

**Solution:** 
```csharp
// Add explicit logging
Log.Information("PaymentSucceeded consumer: OrderId={OrderId}", context.Message.OrderId);

// Verify message received
if (order is null) {
    Log.Warning("Order not found: {OrderId}", context.Message.OrderId);
    return;  // Graceful
}
```

---

### Issue: "Duplicate message processing"

**Cause:** Message delivered twice, no inbox deduplication

**Solution:** Ensure InboxRepository is being used:
```csharp
// In consumer
if (await _inbox.ExistsAsync(messageId, consumer, cancellationToken))
    return;  // Already processed

// ... process ...

await _inbox.AddAsync(messageId, consumer, correlationId, DateTime.UtcNow, cancellationToken);
```

---

## Key Takeaways

### Architectural Principles

✅ **Loose Coupling**
- Services communicate only through events
- No direct service-to-service HTTP calls
- Easy to swap implementations

✅ **High Scalability**
- CatalogService: Stateless, scales horizontally
- OrdersService: Read replicas with eventual consistency
- PaymentsService: Completely stateless

✅ **Reliability**
- Outbox pattern ensures no lost events
- Inbox pattern prevents duplicate processing
- Graceful degradation on errors

✅ **Observability**
- Correlation IDs trace across services
- Structured logging with Serilog
- Distributed tracing with OpenTelemetry

---

### The "Why" Behind Each Pattern

| Pattern | Why It Exists |
|---------|--------------|
| **Outbox** | Guarantee "at least once" event delivery even if service crashes |
| **Inbox** | Prevent duplicate side effects from retried messages |
| **CQRS** | Optimize reads/writes differently (reads are usually heavier) |
| **State Machine** | Enforce valid order transitions, prevent invalid states |
| **MediatR** | Separate commands from side effects, testable handlers |
| **Repository** | Decouple data access from business logic |
| **MassTransit** | Abstraction over RabbitMQ, resilient consumers |

---

### Common Gotchas to Avoid

❌ **Don't:** Call other services via HTTP synchronously
✅ **Do:** Publish events and let services listen asynchronously

❌ **Don't:** Skip the Inbox deduplication check
✅ **Do:** Always record processed messages to handle retries

❌ **Don't:** Update order state directly without domain logic
✅ **Do:** Use Order.MarkAsPaid() to enforce state machine

❌ **Don't:** Forget to call SaveChangesAsync() after publishing events
✅ **Do:** Ensure OutboxMessage persists with application state

❌ **Don't:** Log sensitive data in correlation IDs
✅ **Do:** Use correlation IDs for tracing, not customer data

---

### Deployment Checklist

- [ ] All databases created (CatalogDb, OrdersDb)
- [ ] Migrations applied
- [ ] RabbitMQ running and accessible
- [ ] Connection strings configured correctly
- [ ] OutboxProcessor running in Orders service
- [ ] All consumers registered in MassTransit
- [ ] Retry policies configured appropriately
- [ ] Logging configured (Serilog)
- [ ] OpenTelemetry exporter configured
- [ ] Health checks configured
- [ ] API documentation (Swagger) accessible
- [ ] Load balancing configured if needed

---

### Next Steps for New Team Members

1. **Read This Document** ← You are here
2. **Explore the codebase**
   - Start with `Program.cs` to see DI setup
   - Follow CreateOrderCommand → Handler
   - Trace OrderRequested event consumption
3. **Run locally**
   - `docker-compose up`
   - POST to `/api/checkout`
   - Monitor logs and database changes
4. **Add a small feature**
   - Add a new order field (e.g., notes)
   - Update the state machine
   - Add handling in consumers
5. **Ask questions!**
   - This system is complex by design
   - Asking is better than guessing

---

## Quick Reference Commands

### Docker
```bash
# Start all services
docker-compose up -d

# View logs
docker logs <container-name> -f

# Stop all
docker-compose down

# Rebuild images
docker-compose build
```

### Database
```bash
# Run migrations
dotnet ef database update -p Orders.Infrastructure -s Orders.Api

# Create migration
dotnet ef migrations add <MigrationName> -p Orders.Infrastructure -s Orders.Api

# View migrations
dotnet ef migrations list -p Orders.Infrastructure -s Orders.Api
```

### Testing Flow
```bash
# 1. Create product
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","price":99.99,"categoryId":1}'

# 2. Checkout
curl -X POST http://localhost:5000/api/checkout \
  -H "Content-Type: application/json" \
  -d '{"customerId":"750e8400-e29b-41d4-a716-446655440000","items":[{"productId":"<id>","quantity":1}]}'

# 3. Get order
curl http://localhost:5001/api/orders/<orderId>

# 4. Pay
curl -X POST http://localhost:5001/api/orders/commands/<orderId>/pay

# 5. Check status
curl http://localhost:5001/api/orders/<orderId>
```

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Jan 2025 | Initial comprehensive KT guide |

---

## Contributing

Found an error? Want to improve this guide?

1. Update the appropriate section
2. Run through the flow to verify
3. Update this document
4. Create a PR with documentation improvements

---

## Additional Resources

- **RabbitMQ Management UI:** http://localhost:15672 (guest/guest)
- **Swagger/OpenAPI:**
  - CatalogService: http://localhost:5000/swagger
  - OrdersService: http://localhost:5001/swagger
  - PaymentsService: http://localhost:5002/swagger
- **MassTransit Docs:** https://masstransit.io/
- **Entity Framework Docs:** https://learn.microsoft.com/ef/core/

---

**Last Updated:** January 2025  
**Maintained By:** Architecture Team  
**Status:** Active 🟢
