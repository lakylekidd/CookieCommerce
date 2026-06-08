using Aspire.Hosting;
using CookieCommerce.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("db-password", "MyFixedPassword123!");
var postgres = builder.AddPostgres("postgres").WithPassword(password).WithHostPort(54321);

// Add the databases
var customerDb = postgres.AddDatabase("CustomerDomain");
var productDb = postgres.AddDatabase("ProductDomain");
var authDb = postgres.AddDatabase("AuthDomain");

var rabbit = builder.AddRabbitMQ("Messaging");

// Add Keycloak
var realmsPath = Path.Combine(builder.AppHostDirectory, "realms");

var kcAdminUsername = builder.AddParameter("keycloak-admin-username", "admin");
var kcAdminPassword = builder.AddParameter("keycloak-admin-password", "admin");

var keycloak = builder.AddKeycloak(
    name: "keycloak", 
    port: 8080,
    adminUsername: kcAdminUsername,
    adminPassword: kcAdminPassword)
    .WithRealmImport(realmsPath);

// Add the APIs
var customerApi = builder.AddProject<CookieCommerce_CustomerDomain_Api>("customer-api")
    .WithReference(customerDb)
    .WaitFor(customerDb)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WithScalarUi();
var productApi = builder.AddProject<CookieCommerce_ProductDomain_Api>("product-api")
    .WithReference(productDb)
    .WaitFor(productDb)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WithScalarUi();
var authApi = builder.AddProject<CookieCommerce_AuthDomain_Api>("auth-api")
    .WithReference(authDb).WaitFor(authDb)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithReference(rabbit).WaitFor(rabbit)
    .WithScalarUi();

builder.AddProject<CookieCommerce_Web>("cookiecommerce-web")
    .WithReference(authApi);

builder.Build().Run();
