# CookieCommerce

## TODO

- Add Address to customer
- Create Outbox Pattern
- Wire up DI registrations per Project

## Customer Domain
Customer
 ├── Customer.Api
 ├── Customer.Application
 ├── Customer.Domain
 ├── Customer.Infrastructure
 └── Customer.Contracts

Customer.Api
- Minimal API or controllers
Request → Command/Query mapping
Authentication/authorization
API versioning
OpenAPI/Swagger

Customer.Application
Command handlers (CreateCustomer, UpdateEmail, AddAddress…)
Query handlers (GetCustomerById, SearchCustomers…)
Domain event handlers (internal)
Integration event publishers

Customer.Domain
Aggregates:
Customer
Address (value object)
Domain events:
CustomerRegistered
CustomerEmailUpdated
CustomerAddressAdded
CustomerDeleted
Business rules and invariants

Customer.Infrastructure
Marten configuration (event store + projections)
Repositories
Outbox pattern
Message bus integration
Read model projections

Customer.Contracts
Integration events exposed to other services:
CustomerRegisteredIntegrationEvent
CustomerEmailChangedIntegrationEvent
CustomerDeletedIntegrationEvent