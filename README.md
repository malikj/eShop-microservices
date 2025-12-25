# eShop Microservices (.NET)

A modern **.NET-based microservices e-commerce solution** built as a hands-on learning project, following real-world architecture principles and cloud-ready practices.

This repository is designed to help deepen expertise in:
- .NET Microservices
- Clean Architecture
- Docker & Containers
- Event-driven communication
- Cloud-native development (Azure-ready)

---

## 🏗️ Architecture Overview

The solution is composed of multiple independently deployable microservices, each responsible for a specific business capability.

### Current Services
- **CatalogService**
  - Manages product catalog data
- **Orders**
  - Handles order creation and processing
- **eShop.Contracts**
  - Shared contracts (DTOs, events, interfaces)

Services communicate using well-defined contracts and are designed to be containerized and scalable.

---

## 🧰 Technology Stack

- **.NET 8 / ASP.NET Core**
- **REST APIs**
- **Docker & Docker Compose**
- **Entity Framework Core**
- **SQL Server / PostgreSQL (pluggable)**
- **GitHub**
- **(Planned)** Message broker (RabbitMQ / Azure Service Bus)
- **(Planned)** Azure Container Apps / AKS

---

## 📂 Project Structure

```text
eShop-microservices
│
├── CatalogService/
├── Orders/
├── eShop.Contracts/
│
├── docker-compose.yml
├── .gitignore
└── README.md
