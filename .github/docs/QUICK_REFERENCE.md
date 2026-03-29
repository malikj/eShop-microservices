# eShop Microservices - Quick Reference Card

**Print this page or bookmark for rapid lookup**

---

## ?? The System in One Diagram

```
User ? CatalogService ? [OrderRequested] ? OrdersService ? [OrderPaymentRequested]
                                                  ?
                                              [PaymentSucceeded/Failed]
                                                  ?
                            PaymentsService ? [OrderPaymentRequested]
```

---

## ?? Service Locations

| Service | Port | Database | Purpose |
|---------|------|----------|---------|
| CatalogService | 5000 | CatalogDb | Products + Checkout |
| OrdersService | 5001 | OrdersDb | Order Orchestration |
| PaymentsService | 5002 | None | Payment Validation |
| RabbitMQ | 5672 | N/A | Message Bus |
| RabbitMQ UI | 15672 | N/A | Management Console |

---

## ?? Key Concepts at a Glance

| Concept | What It Does | Where It Is |
|---------|-------------|-------------|
| **Order** | State machine entity | Orders.Domain.Entities.Order |
| **Outbox Pattern** | Reliable event publishing | MassTransitEventPublisher |
| **Inbox Pattern** | Idempotent processing | InboxRepository |
| **CQRS** | Separate read/write | IOrderRepository + IOrderReadRepository |
| **MediatR** | Command handling | CreateOrderCommandHandler |
| **Consumer** | Event listener | OrderRequestedConsumer |

---

## ??? File Locations (By Feature)

### Creating an Order
```
CatalogService/Catalog.Api/Controllers/CheckoutController.cs
CatalogService/Catalog.Application/Checkout/CheckoutService.cs
Orders/Orders.Api/Consumers/OrderRequestedConsumer.cs
Orders/Orders.Application/Ordering/Commands/CreateOrder/CreateOrderCommandHandler.cs
```

### Paying for an Order
```
Orders/Orders.Api/Controllers/OrdersCommandController.cs
Orders/Orders.Application/Ordering/Commands/PayOrder/PayOrderCommandHandler.cs
Payments/Payments.Api/Consumers/OrderPaymentRequestedConsumer.cs
Orders/Orders.Api/Consumers/PaymentSucceededConsumer.cs
```

### Publishing Events
```
Orders/Orders.Infrastructure/Messaging/MassTransitEventPublisher.cs
Orders/Orders.Infrastructure/Messaging/OutboxProcessor.cs
Orders/Orders.Infrastructure/Persistence/OutboxMessage.cs
```

### Order State Management
```
Orders/Orders.Domain/Entities/Order.cs
Orders/Orders.Domain/Enums/OrderStatus.cs
Orders/Orders.Api/Consumers/PaymentSucceededConsumer.cs
Orders/Orders.Api/Consumers/PaymentFailedConsumer.cs
```

---

## ?? Order Status Transitions

```
CREATED (Pending)
  ? [PaymentSucceeded event]
  ?? PAID (ready to ship)
  ?
  ? [PaymentFailed event]
  ?? CANCELLED (payment rejected)
```

---

## ?? Database Schemas (Quick View)

### Orders Table
```sql
Id (Guid) | CustomerId (Guid) | Status (int) | TotalPrice (decimal) | CreatedAt
```

### OrderItems Table
```sql
ProductId | ProductName | UnitPrice | Quantity | OrderId (FK)
```

### OutboxMessages Table
```sql
Id | Type | Content (JSON) | OccurredOnUtc | ProcessedOnUtc | Error
```

### InboxMessages Table
```sql
MessageId | Consumer | CorrelationId | ProcessedAt
```

---

## ?? API Endpoints (All Services)

### CatalogService
```
POST   /api/checkout                  # Trigger order creation
GET    /api/products                  # List products
POST   /api/products                  # Create product
GET    /api/categories                # List categories
POST   /api/categories                # Create category
```

### OrdersService
```
GET    /api/orders                    # List all orders
GET    /api/orders?customerId={id}    # List customer's orders
GET    /api/orders/{orderId}          # Get order details
POST   /api/orders/commands/{id}/pay  # Initiate payment
POST   /api/orders/commands/{id}/cancel  # Cancel order
```

### PaymentsService
```
(No HTTP endpoints - event consumer only)
```

---

## ?? Event Contracts (Payload Structure)

### OrderRequested
```json
{
  "orderId": "guid",
  "customerId": "guid",
  "createdAt": "datetime",
  "totalAmount": 9999,
  "items": [{"productId": "guid", "productName": "string", "unitPrice": 5000, "quantity": 1}]
}
```

### OrderPaymentRequested
```json
{
  "orderId": "guid",
  "customerId": "guid",
  "amount": 9999
}
```

### PaymentSucceeded
```json
{
  "orderId": "guid",
  "paidAt": "datetime"
}
```

### PaymentFailed
```json
{
  "orderId": "guid",
  "reason": "string"
}
```

---

## ?? Startup Configuration (Program.cs Checklist)

```csharp
// Database
? AddDbContext<OrdersDbContext>()

// Repositories
? AddScoped<IOrderRepository>()
? AddScoped<IOrderReadRepository>()
? AddScoped<IInboxRepository>()

// MassTransit
? AddMassTransit(x => x.AddConsumer<...>())
? ConfigureRabbitMq()

// Handlers
? AddMediatR()

// Background Services
? AddHostedService<OutboxProcessor>()

// Observability
? AddScoped<ICorrelationIdAccessor>()
? UseSerilog()
? AddOpenTelemetry()
```

---

## ?? Quick Troubleshooting

| Problem | Check |
|---------|-------|
| Order stuck in Pending | Is PaymentSucceededConsumer registered? |
| RabbitMQ connection error | `appsettings.json` - Host/credentials |
| OutboxMessages not publishing | Is OutboxProcessor running? |
| Duplicate processing | Check InboxMessages table |
| Service won't start | Missing AddHostedService<>()? |
| Events not received | Consumer registered in MassTransit? |

---

## ?? Common Patterns

### Create an Order (Three Steps)
```csharp
1. Publish OrderRequested event
   ? OrderRequestedConsumer picks it up
   ? MediatR CreateOrderCommandHandler creates Order
   ? Saves to database

2. User initiates payment
   ? PayOrderCommandHandler publishes OrderPaymentRequested
   ? OutboxProcessor publishes to RabbitMQ

3. Update order status
   ? PaymentSucceededConsumer receives event
   ? Checks Inbox (idempotency)
   ? Updates Order.Status = Paid
```

### Process an Event (Two Checks)
```csharp
1. Idempotency Check:
   if (await inbox.ExistsAsync(messageId, consumer))
       return;  // Already processed

2. Process:
   // ... update state ...
   await inbox.AddAsync(messageId, consumer, ...);
   await SaveChangesAsync();
```

---

## ?? Key Files by Layer

### Domain
```
Orders.Domain/Entities/Order.cs
Orders.Domain/Entities/OrderItem.cs
Orders.Domain/Enums/OrderStatus.cs
```

### Application
```
Orders.Application/Ordering/Commands/CreateOrder/CreateOrderCommandHandler.cs
Orders.Application/Ordering/Commands/PayOrder/PayOrderCommandHandler.cs
Orders.Application/Abstractions/Repositories/IOrderRepository.cs
Orders.Application/Abstractions/Messaging/IEventPublisher.cs
```

### Infrastructure
```
Orders.Infrastructure/Persistence/OrdersDbContext.cs
Orders.Infrastructure/Messaging/MassTransitEventPublisher.cs
Orders.Infrastructure/Messaging/OutboxProcessor.cs
Orders.Infrastructure/Repositories/OrderRepository.cs
Orders.Infrastructure/Repositories/InboxRepository.cs
```

### API
```
Orders.Api/Program.cs
Orders.Api/Controllers/OrdersCommandController.cs
Orders.Api/Consumers/OrderRequestedConsumer.cs
Orders.Api/Consumers/PaymentSucceededConsumer.cs
Orders.Api/Consumers/PaymentFailedConsumer.cs
```

---

## ?? One-Minute Flows

### Checkout to Order Creation (4 steps)
```
1. POST /api/checkout
2. CatalogService publishes OrderRequested
3. OrderRequestedConsumer creates Order (Status: Pending)
4. Order visible at GET /api/orders/{id}
```

### Payment Processing (4 steps)
```
1. POST /api/orders/{id}/pay
2. PayOrderCommandHandler publishes OrderPaymentRequested (via Outbox)
3. PaymentsService validates amount
4. PaymentSucceededConsumer marks Order.Status = Paid
```

---

## ? Pre-Deployment Checklist

- [ ] Databases created (CatalogDb, OrdersDb)
- [ ] Migrations applied
- [ ] RabbitMQ running
- [ ] Connection strings correct
- [ ] OutboxProcessor registered
- [ ] Consumers registered
- [ ] Retry policies configured
- [ ] Logging configured
- [ ] OpenTelemetry configured

---

## ?? Quick Links

| Resource | URL |
|----------|-----|
| RabbitMQ UI | http://localhost:15672 |
| Catalog Swagger | http://localhost:5000/swagger |
| Orders Swagger | http://localhost:5001/swagger |
| Payments Swagger | http://localhost:5002/swagger |

---

## ?? Decision Matrix: Which Service Does What?

| Task | Service |
|------|---------|
| Add a product? | CatalogService |
| Get product details? | CatalogService |
| Create an order? | OrdersService (via event) |
| Update order status? | OrdersService (via consumer) |
| Validate payment amount? | PaymentsService |
| Check order history? | OrdersService (read repository) |

---

## ?? Mental Models (Quick Analogies)

| Concept | Analogy |
|---------|---------|
| **Outbox** | A mailbox: write letter and put in mailbox (one action), postman delivers later |
| **Inbox** | A checklist: mark items as done so you don't repeat them |
| **Consumer** | A waiter listening for a bell ring, then responding |
| **Event** | A megaphone announcement everyone can hear |
| **State Machine** | Traffic light: can only go green?red?yellow, not backwards |
| **CQRS** | Writing to a logbook vs reading summaries (different optimization) |

---

## ?? Security Considerations

- [ ] RabbitMQ credentials rotated?
- [ ] Database credentials never in code?
- [ ] Correlation IDs don't contain sensitive data?
- [ ] Events don't contain passwords/PII?
- [ ] SQL injection protected (ORM used)?
- [ ] Unauthorized API access restricted?

---

**Bookmark This! ??**

Print page or save locally for instant reference during development.

**Last Updated:** January 2025
