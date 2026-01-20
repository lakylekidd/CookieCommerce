using Projects;

var builder = DistributedApplication.CreateBuilder(args);


var postgres = builder.AddPostgres("postgres");
var customerDb = postgres.AddDatabase("CustomerDomain");

var rabbit = builder.AddRabbitMQ("Messaging");

var customerApi = builder.AddProject<CookieCommerce_CustomerDomain_Api>("customer-api")
    .WithReference(customerDb)
    .WithReference(rabbit);


builder.Build().Run();
