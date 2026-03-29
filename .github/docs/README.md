# eShop Microservices - Documentation Hub

**Complete knowledge transfer package for new team members**

---

## ?? Documentation Overview

Welcome to the eShop microservices project! This documentation package provides everything you need to understand, develop, and maintain the system.

### Quick Navigation

| Document | Purpose | Best For |
|----------|---------|----------|
| **[MICROSERVICES_KT_GUIDE.md](#)** | Comprehensive technical overview | Deep learning, architecture understanding |
| **[QUICK_REFERENCE.md](#)** | One-page lookup guide | Fast reference during coding |
| **[ARCHITECTURE_DIAGRAMS.md](#)** | Visual flowcharts and timelines | Visual learners, system design |
| **This README** | Getting started guide | First-time onboarding |

---

## ?? Getting Started (5 Minutes)

### What is eShop?

eShop is a **microservices learning system** that demonstrates:
- Clean Architecture
- Event-driven design
- CQRS pattern
- Transactional Outbox
- Idempotent message processing

### The Big Picture

```
Customer clicks "Checkout"
    ?
CatalogService publishes OrderRequested
    ?
OrdersService creates Order (Status: Pending)
    ?
Customer clicks "Pay"
    ?
OrdersService publishes OrderPaymentRequested
    ?
PaymentsService validates amount
    ?
PaymentSucceeded event published
    ?
OrdersService marks Order.Status = Paid
    ? Done!
```

### Three Microservices

1. **CatalogService** (Port 5000)
   - Manages products and categories
   - Handles checkout requests
   - Publishes `OrderRequested` events

2. **OrdersService** (Port 5001)
   - Manages order lifecycle
   - Orchestrates payment processing
   - Maintains order state

3. **PaymentsService** (Port 5002)
   - Validates payment amounts
   - Publishes success/failure events
   - Completely stateless

---

## ??? Setup in 5 Steps

### Prerequisites
```bash
# .NET 8 SDK
# Docker & Docker Compose
# Visual Studio 2022 or VS Code
```

### Installation

```bash
# 1. Clone repository
git clone https://github.com/malikj/eShop-microservices
cd eShop

# 2. Start infrastructure (SQL Server + RabbitMQ)
docker-compose up -d

# 3. Apply database migrations
dotnet ef database update -p CatalogService/Catalog.Infrastructure -s CatalogService/Catalog.Api
dotnet ef database update -p Orders/Orders.Infrastructure -s Orders/Orders.Api

# 4. Start services
cd CatalogService/Catalog.Api && dotnet run
cd ../../Orders/Orders.Api && dotnet run
cd ../../Payments/Payments.Api && dotnet run

# 5. Try it out
curl -X POST http://localhost:5000/api/checkout
```

### Verify Installation

```bash
# Check APIs are running
curl http://localhost:5000/swagger   # CatalogService
curl http://localhost:5001/swagger   # OrdersService
curl http://localhost:5002/swagger   # PaymentsService

# Check RabbitMQ
open http://localhost:15672          # guest/guest

# Check databases
# Connect to localhost:1433 with SQL Server Management Studio
```

---

## ?? Learning Path

### Day 1: Understand the Architecture
1. Read: [MICROSERVICES_KT_GUIDE.md](MICROSERVICES_KT_GUIDE.md) - Sections 1-3
2. Watch: Architecture overview (if available)
3. Run: Local setup + test `/api/checkout` flow
4. Explore: Open `Orders/Orders.Domain/Entities/Order.cs` - understand the Order entity

### Day 2: Deep Dive into Order Creation
1. Read: [MICROSERVICES_KT_GUIDE.md](MICROSERVICES_KT_GUIDE.md) - Section 4 (Complete User Journey)
2. Trace: Follow the code path:
   - `CatalogService/Catalog.Api/Controllers/CheckoutController.cs`
   - `CatalogService/Catalog.Application/Checkout/CheckoutService.cs`
   - `Orders/Orders.Api/Consumers/OrderRequestedConsumer.cs`
   - `Orders/Orders.Application/Ordering/Commands/CreateOrder/CreateOrderCommandHandler.cs`
3. Understand: How OrderRequested event triggers order creation

### Day 3: Event-Driven Patterns
1. Read: [MICROSERVICES_KT_GUIDE.md](MICROSERVICES_KT_GUIDE.md) - Section 5 (Core Patterns)
2. Study: 
   - Transactional Outbox: `Orders/Orders.Infrastructure/Messaging/MassTransitEventPublisher.cs`
   - Inbox Deduplication: `Orders/Orders.Infrastructure/Repositories/InboxRepository.cs`
   - State Machine: `Orders/Orders.Domain/Entities/Order.cs`
3. Experiment: Add logging to trace event flow

### Day 4: Payment Processing
1. Read: [MICROSERVICES_KT_GUIDE.md](MICROSERVICES_KT_GUIDE.md) - Section 6 (Data & Logic Flow)
2. Code Walk: Payment request ? validation ? response
   - `Orders/Orders.Application/Ordering/Commands/PayOrder/PayOrderCommandHandler.cs`
   - `Payments/Payments.Api/Consumers/OrderPaymentRequestedConsumer.cs`
   - `Orders/Orders.Api/Consumers/PaymentSucceededConsumer.cs`
3. Test: Try paying for an order, check database state changes

### Day 5: Hands-On Development
1. Task: Add a new field to Order (e.g., `OrderNotes`)
   - Update domain entity
   - Create migration
   - Update handlers
   - Test flow
2. Reference: Use [QUICK_REFERENCE.md](QUICK_REFERENCE.md) for quick lookups

---

## ?? Common Tasks

### Add a New Product

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Awesome Widget",
    "description": "The best widget",
    "price": 29.99,
    "categoryId": 1
  }'
```

**Code:**
- `CatalogService/Catalog.Api/Controllers/ProductsController.cs`
- `CatalogService/Catalog.Application/Products/ProductService.cs`

### Trace an Order Through the System

```bash
# 1. Create order (copy orderId from response)
curl -X POST http://localhost:5000/api/checkout \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "750e8400-e29b-41d4-a716-446655440000",
    "items": [{"productId": "<productId>", "quantity": 1}]
  }'

# 2. Check order exists (Status: Pending)
curl http://localhost:5001/api/orders/<orderId>

# 3. Pay for order
curl -X POST http://localhost:5001/api/orders/commands/<orderId>/pay

# 4. Check order updated (Status: Paid)
curl http://localhost:5001/api/orders/<orderId>
```

### Debug: Order Stuck in Pending

**Checklist:**
1. Check RabbitMQ queues: http://localhost:15672
2. Check logs: Look for PaymentSucceededConsumer errors
3. Query database:
   ```sql
   SELECT * FROM Orders WHERE Id = '<orderId>'
   SELECT * FROM OutboxMessages WHERE ProcessedOnUtc IS NULL
   SELECT * FROM InboxMessages WHERE MessageId LIKE '%'
   ```
4. Check consumer registration in `Orders/Orders.Api/Program.cs`

---

## ?? Code Navigation Guide

### Finding Things

| What You Want | Where to Look |
|---------------|---------------|
| Order entity logic | `Orders/Orders.Domain/Entities/Order.cs` |
| Create order handler | `Orders/Orders.Application/Ordering/Commands/CreateOrder/CreateOrderCommandHandler.cs` |
| Pay order handler | `Orders/Orders.Application/Ordering/Commands/PayOrder/PayOrderCommandHandler.cs` |
| Order-related consumers | `Orders/Orders.Api/Consumers/` |
| Database context | `Orders/Orders.Infrastructure/Persistence/OrdersDbContext.cs` |
| Event publishing | `Orders/Orders.Infrastructure/Messaging/MassTransitEventPublisher.cs` |
| Outbox processing | `Orders/Orders.Infrastructure/Messaging/OutboxProcessor.cs` |
| Event definitions | `eShop.Contracts/Events/` |

### File Naming Conventions

```
CatalogService/
??? Catalog.Domain/           ? Business entities (Product, Category)
??? Catalog.Application/      ? Use cases (ProductService, CheckoutService)
??? Catalog.Infrastructure/   ? Data access (ProductRepository, DbContext)
??? Catalog.Api/              ? HTTP endpoints (ProductsController)

Orders/
??? Orders.Domain/            ? Business entities (Order, OrderItem)
??? Orders.Application/       ? Use cases (Handlers, Services)
??? Orders.Infrastructure/    ? Data access (Repositories, DbContext, Messaging)
??? Orders.Api/               ? HTTP endpoints, Consumers, Middleware
```

---

## ?? Architecture Quick Reference

### Layers (Clean Architecture)

```
API Layer          ? REST endpoints, consumers, middleware
Application Layer  ? Commands, queries, services, DTOs
Domain Layer       ? Entities, enums, repository interfaces
Infrastructure     ? DbContext, repository implementations, messaging
```

### Key Patterns

```
Outbox Pattern     ? Reliable event publishing (writes to DB first)
Inbox Pattern      ? Idempotent processing (track processed messages)
CQRS Pattern       ? Separate read/write models
State Machine      ? Valid order transitions (Pending ? Paid ? Shipped)
MediatR            ? Command/query request handler pattern
```

### Communication

```
Synchronous:  REST APIs (HTTP)
Asynchronous: RabbitMQ events (async/await with consumers)
```

---

## ?? Testing the System

### Manual Testing (Recommended for learning)

```bash
# Terminal 1: CatalogService
cd CatalogService/Catalog.Api
dotnet run

# Terminal 2: OrdersService
cd Orders/Orders.Api
dotnet run

# Terminal 3: PaymentsService
cd Payments/Payments.Api
dotnet run

# Terminal 4: Run test commands
# [Follow Quick Setup section above]
```

### Checking Results

**After checkout:**
```sql
SELECT * FROM Orders WHERE Status = 1;  -- 1 = Pending
SELECT COUNT(*) FROM OutboxMessages WHERE ProcessedOnUtc IS NULL;
```

**After payment:**
```sql
SELECT * FROM Orders WHERE Status = 2;  -- 2 = Paid
SELECT COUNT(*) FROM InboxMessages;
```

---

## ?? Troubleshooting

### RabbitMQ Connection Error

**Error:** `RabbitMQ connection refused`

**Fix:**
```bash
docker-compose up -d rabbitmq
# Wait 30 seconds
# Restart services
```

### Database Connection Error

**Error:** `Cannot connect to server at 'localhost:1433'`

**Fix:**
```bash
docker-compose up -d sql-server
# Wait 30 seconds
# Run migrations again
```

### Order Stuck in Pending

**Symptom:** Order created but never transitions to Paid

**Diagnose:**
1. Check OutboxMessages table (should be mostly empty)
2. Check RabbitMQ queues (should be empty)
3. Check application logs for exceptions
4. Verify PaymentSucceededConsumer is registered

---

## ?? Advanced Reading

### For Deeper Understanding

- **Design Patterns**
  - [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html) by Martin Fowler
  - [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html) by Chris Richardson
  - [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html) by Martin Fowler

- **Technology Docs**
  - [MassTransit Documentation](https://masstransit.io/)
  - [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
  - [RabbitMQ Tutorials](https://www.rabbitmq.com/tutorials)

- **Books**
  - "Building Microservices" by Sam Newman
  - "Patterns of Enterprise Application Architecture" by Martin Fowler
  - "Domain-Driven Design" by Eric Evans

---

## ? Onboarding Checklist

Use this checklist as you onboard:

- [ ] Read MICROSERVICES_KT_GUIDE.md sections 1-3
- [ ] Set up local environment (Docker + .NET)
- [ ] Run all three services locally
- [ ] Test checkout flow (POST /api/checkout)
- [ ] Test order payment flow
- [ ] Query database directly to verify state changes
- [ ] Read MICROSERVICES_KT_GUIDE.md sections 4-5
- [ ] Trace code path: checkout ? order creation ? payment
- [ ] Understand Outbox pattern
- [ ] Understand Inbox pattern
- [ ] Understand state machine
- [ ] Read ARCHITECTURE_DIAGRAMS.md
- [ ] Add a new field to an order (hands-on)
- [ ] Run all tests
- [ ] Code review with mentor
- [ ] Deploy to development environment
- [ ] Ready to contribute!

---

## ?? Contributing

### Before Making Changes

1. Create a feature branch
2. Make changes following code style
3. Update relevant documentation
4. Add tests for new features
5. Ensure all tests pass
6. Create pull request
7. Request code review

### Code Style

- Follow C# conventions
- Use meaningful variable names
- Add XML documentation for public methods
- Keep methods focused and testable
- Use repository pattern for data access

---

## ?? Getting Help

### Resources

- **Documentation:** This package (you're reading it!)
- **Code Examples:** Look at existing handlers/consumers as templates
- **Questions:** Ask a senior team member

### Common Questions

**Q: How do I add a new event?**
A: Define record in `eShop.Contracts/Events/`, implement publisher, create consumer

**Q: How do I trace a request?**
A: Use correlation ID in logs or query database directly

**Q: How do I debug a failing consumer?**
A: Add logging statements, check RabbitMQ UI, look at application logs

---

## ?? Next Steps

1. ? Read this README
2. ? Set up local environment
3. ? Read [MICROSERVICES_KT_GUIDE.md](MICROSERVICES_KT_GUIDE.md)
4. ? Read [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
5. ? Study [ARCHITECTURE_DIAGRAMS.md](ARCHITECTURE_DIAGRAMS.md)
6. ? Complete onboarding checklist
7. ? Your first pull request!

---

## ?? Documentation Status

| Document | Status | Last Updated |
|----------|--------|--------------|
| MICROSERVICES_KT_GUIDE.md | ? Complete | Jan 2025 |
| QUICK_REFERENCE.md | ? Complete | Jan 2025 |
| ARCHITECTURE_DIAGRAMS.md | ? Complete | Jan 2025 |
| README.md | ? Complete | Jan 2025 |

---

## ?? Success Criteria

You'll know you're ready when you can:

1. ? Explain the three microservices and their roles
2. ? Trace a complete order flow from checkout to payment
3. ? Explain the Outbox and Inbox patterns
4. ? Add a new field to an Order entity
5. ? Debug a failing consumer
6. ? Write a new handler for a command
7. ? Create a pull request with confidence

---

## ?? Support

For questions or issues:
1. Check the troubleshooting section
2. Search existing documentation
3. Ask a team member
4. Check GitHub issues

---

**Welcome to the team! ??**

You now have everything you need to become productive. Start with the README, move to the comprehensive guide, reference the quick card, and consult the diagrams. 

Happy coding!

---

**Document Maintained By:** Architecture Team  
**Last Updated:** January 2025  
**Status:** Active ??
