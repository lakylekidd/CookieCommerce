## Goal
Seed users created by `KeycloakBootstrapService` (and all future registrations) must also exist as `CustomerAggregate` records in the CustomerDomain. The fix is event-driven: `RegisterUserHandler` publishes a `UserRegisteredIntegrationEvent` that `CustomerDomain.Infrastructure` consumes to call `CreateCustomer`.

## Architecture
- `KeycloakBootstrapService` already calls `RegisterUserHandler` via MediatR — no changes needed there.
- `RegisterUserHandler` gains `IIntegrationEventPublisher` and publishes `UserRegisteredIntegrationEvent` after a successful registration.
- `CookieMassTransit.AddCookieMassTransit` gains an optional `Action<IBusRegistrationConfigurator>` parameter so callers (CustomerDomain.Api) can register consumers without duplicating all the transport wiring.
- `CustomerDomain.Infrastructure` gets a `UserRegisteredConsumer` that sends `CreateCustomer` (Gender.Unknown, null DateOfBirth as defaults).
- `CustomerDomain.Infrastructure.csproj` gets a project reference to `AuthenticationManagement.Application` so both sides use the same CLR type → same MassTransit exchange name. Dependency direction: Customer (downstream/conformist) → Auth (upstream), one-way only.
- `CustomerDomain.Api/Program.cs` passes the consumer hook into `AddCookieMassTransit`.

## Key files
| File | Change |
|------|--------|
| `AuthenticationManagement.Application/Auth/Integration/UserRegisteredIntegrationEvent.cs` | NEW — event contract |
| `AuthenticationManagement.Application/Auth/Handlers/RegisterUserHandler.cs` | inject publisher, publish event |
| `CookieMassTransit/DependencyInjection.cs` | add optional `configureConsumers` param |
| `CustomerDomain.Infrastructure/Consumers/UserRegisteredConsumer.cs` | NEW — calls CreateCustomer |
| `CustomerDomain.Infrastructure/DependencyInjection.cs` | expose `AddCustomerDomainConsumers(IBusRegistrationConfigurator)` |
| `CustomerDomain.Infrastructure/CookieCommerce.CustomerDomain.Infrastructure.csproj` | add ref to AuthManagement.Application + MassTransit.RabbitMQ |
| `CustomerDomain.Api/Program.cs` | pass consumer hook to AddCookieMassTransit |

## Verification
After changes, running the Aspire AppHost should show bootstrap log lines for both auth AND customer creation. No new test project needed in this plan.
