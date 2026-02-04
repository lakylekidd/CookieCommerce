using Aspire.Hosting;
using CookieCommerce.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("db-password", "MyFixedPassword123!");
var postgres = builder.AddPostgres("postgres").WithPassword(password).WithHostPort(54321);
var customerDb = postgres.AddDatabase("CustomerDomain");

var rabbit = builder.AddRabbitMQ("Messaging");

var customerApi = builder.AddProject<CookieCommerce_CustomerDomain_Api>("customer-api")
    .WithReference(customerDb)
    .WaitFor(customerDb)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WithScalarUi();

builder.AddProject<Projects.CookieCommerce_Web>("cookiecommerce-web");

builder.Build().Run();
