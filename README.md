# StockFlow – Inventory Management System

StockFlow is a backend-focused **Inventory Management System** built with **ASP.NET Core Web API**.

The project was created to practice real-world backend development concepts such as inventory tracking, order processing, stock reservations, authentication, caching, real-time updates, background jobs, and clean architecture principles.

---

## Main Features

### Inventory Management

* Manage products across multiple warehouses.
* Track:

  * On-hand quantity
  * Reserved quantity
  * Available quantity
  * Reorder level
* Prevent negative stock and invalid inventory operations.

Example:

```text
On Hand = 100
Reserved = 30
Available = 70
```

---

### Products, Categories & Warehouses

The system supports managing:

* Products
* Categories
* Warehouses
* Inventory records for each Product-Warehouse combination

---

### Order Management

StockFlow handles the main order workflow:

```text
Create Order
     ↓
Add Order Items
     ↓
Check Inventory
     ↓
Reserve Stock
     ↓
Fulfill Order
     ↓
Update Inventory
```

Business rules are handled inside the Service Layer instead of directly inside controllers.

---

### Stock Reservation

The system reserves stock before completing an order instead of immediately deducting it.

Reservation statuses include:

```text
Active
Released
Expired
Cancelled
```

This helps prevent multiple orders from using the same available stock.

---

### Inventory Transactions

Every important inventory change can be recorded as an inventory transaction.

Examples:

* Stock In
* Stock Out
* Reservation
* Reservation Release
* Order Fulfillment
* Inventory Adjustment

This creates an audit history for inventory changes.

---

## Authentication & Authorization

The project uses:

* ASP.NET Core Identity
* JWT Authentication
* Refresh Tokens
* Role-Based Authorization

Main authentication flow:

```text
Login
  ↓
Generate Access Token
  ↓
Generate Refresh Token
  ↓
Access Protected Endpoints
```

Protected endpoints can use:

```csharp
[Authorize]
```

or:

```csharp
[Authorize(Roles = "Admin")]
```

---

## Redis Caching

StockFlow uses **Redis** as a distributed cache.

The project follows the Cache-Aside approach:

```text
Request
   ↓
Check Redis
   ↓
Cache Hit → Return Data

Cache Miss
   ↓
Query Database
   ↓
Store in Redis
   ↓
Return Data
```

Caching is handled through:

```text
ICacheService
      ↓
RedisCacheService
```

The system also removes outdated cache entries when data changes.

---

## Real-Time Updates with SignalR

SignalR is used to notify connected clients when inventory data changes.

Example:

```text
Inventory Updated
       ↓
Backend
       ↓
SignalR Hub
       ↓
Connected Clients
```

This allows the frontend to receive live inventory updates without constantly requesting the API.

---

## Background Jobs with Hangfire

StockFlow uses **Hangfire** for background processing.

One important job is reservation expiration.

```text
Reservation Created
        ↓
Expiration Time
        ↓
Hangfire Job
        ↓
Reservation Expired
        ↓
Reserved Stock Released
```

The project includes:

* Recurring Jobs
* Automatic Retry
* Hangfire Dashboard
* SQL Server Storage
* Concurrent execution protection

---

## Error Handling

The project uses centralized exception handling through a custom middleware.

Instead of repeating `try/catch` blocks inside every controller:

```text
Request
   ↓
Global Exception Middleware
   ↓
Controller
   ↓
Service
```

Errors are returned in a consistent format.

Example:

```json
{
  "status": 404,
  "title": "Not Found",
  "message": "Inventory not found.",
  "traceId": "..."
}
```

The API handles responses such as:

* 400 Bad Request
* 401 Unauthorized
* 403 Forbidden
* 404 Not Found
* 409 Conflict
* 500 Internal Server Error

---

## Validation

The project includes input and business validation such as:

* Product must exist.
* Warehouse must exist.
* Quantity must be greater than zero.
* Inventory must exist.
* Available stock must be enough before reservation.
* Inventory quantities cannot become negative.

---

## Architecture

StockFlow follows a layered architecture:

```text
Client
  ↓
Controllers
  ↓
Services
  ↓
Repositories
  ↓
Entity Framework Core
  ↓
SQL Server
```

Other infrastructure services work alongside the main architecture:

```text
Redis   → Caching
SignalR → Real-Time Updates
Hangfire → Background Jobs
```

---

## Project Structure

```text
StockFlow
│
├── Controllers
├── Services
├── Interfaces
├── Repositories
├── Models
├── DTOs
├── Data
├── Middleware
├── Hubs
├── BackgroundJobs
└── Program.cs
```

---

## Backend Patterns & Concepts

The project applies:

* Repository Pattern
* Generic Repository Pattern
* Service Layer
* Dependency Injection
* DTO Pattern
* Cache-Aside Pattern
* Separation of Concerns
* SOLID Principles
* Async/Await
* Interface-Based Design

---

## Entity Framework Core

The project uses **Entity Framework Core Code First** with SQL Server.

Main concepts used:

* DbContext
* DbSet
* Migrations
* Relationships
* Navigation Properties
* LINQ
* Async Queries
* Include
* AsNoTracking

Examples:

```csharp
Where()
Select()
Include()
Any()
FirstOrDefaultAsync()
ToListAsync()
OrderByDescending()
AsNoTracking()
```

---

## Docker

Docker is used mainly to run infrastructure services such as Redis.

The project includes experience with:

```text
Docker Desktop
Docker Engine
Containers
Redis Container
```

---

## Technologies

### Backend

* C#
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* LINQ

### Authentication

* ASP.NET Core Identity
* JWT
* Refresh Tokens
* Role-Based Authorization

### Infrastructure

* Redis
* SignalR
* Hangfire
* Docker

### API Tools

* Swagger / OpenAPI

---

## Key Backend Concepts Covered

StockFlow helped practice:

* REST API Development
* Database Design
* CRUD Operations
* Repository Pattern
* Service Layer
* DTOs
* Dependency Injection
* Async Programming
* Authentication
* Authorization
* JWT
* Refresh Tokens
* Inventory Management
* Order Processing
* Stock Reservations
* Inventory Transactions
* Redis Caching
* Cache Invalidation
* Serialization / Deserialization
* SignalR
* Real-Time Communication
* Background Jobs
* Hangfire
* Global Exception Handling
* Validation
* Logging
* Business Logic
* Concurrency Awareness

---

## Business Rules

Some important business rules implemented in the project:

* Stock cannot be reserved when available quantity is insufficient.
* Reserved stock is different from physical on-hand stock.
* Inventory cannot become negative.
* Expired reservations automatically release reserved stock.
* Inventory changes can generate transaction records.
* Protected endpoints require authentication and authorization.

---

## Running the Project

### Requirements

```text
.NET SDK
SQL Server
Redis
Docker Desktop
```

Clone the repository:

```bash
git clone <YOUR_REPOSITORY_URL>
```

Navigate to the project:

```bash
cd StockFlow
```

Restore packages:

```bash
dotnet restore
```

Apply migrations:

```bash
dotnet ef database update
```

Run the application:

```bash
dotnet run
```

Swagger will be available through the development URL shown in the terminal.

---

## Author

**Nour Mohamed**

Full Stack .NET Developer

**Tech Stack:** ASP.NET Core, C#, Entity Framework Core, SQL Server, Redis, SignalR, Hangfire, JWT, Docker
