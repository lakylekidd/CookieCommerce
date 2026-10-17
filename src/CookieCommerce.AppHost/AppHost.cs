using Aspire.Hosting;
using CookieCommerce.AppHost;
using Projects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var brandbookEnabled = builder.Environment.IsDevelopment() && builder.Configuration.GetValue("Brandbook:Enabled", false);

var password = builder.AddParameter("db-password", "MyFixedPassword123!");
var volumePrefix = builder.Configuration["DevelopmentStorage:VolumePrefix"] ?? "cookiecommerce";
var postgres = builder.AddPostgres("postgres").WithPassword(password).WithHostPort(builder.Configuration.GetValue("DevelopmentStorage:PostgresPort", 54321))
    .WithDataVolume(volumePrefix + "-postgres-data").WithLifetime(ContainerLifetime.Persistent);

// Add the databases
var customerDb = postgres.AddDatabase("CustomerDomain");
var productDb = postgres.AddDatabase("ProductDomain");
var commerceDb = postgres.AddDatabase("Commerce");
var authenticationManagementDb = postgres.AddDatabase("AuthenticationManagement");

var rabbit = builder.AddRabbitMQ("Messaging").WithDataVolume(volumePrefix + "-rabbitmq-data").WithLifetime(ContainerLifetime.Persistent);

// Add Keycloak
var realmsPath = Path.Combine(builder.AppHostDirectory, "realms");
var localStoragePath = builder.Configuration["DevelopmentStorage:RootPath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CookieCommerce");
var productMediaPath = Path.Combine(localStoragePath, "ProductMedia");
var keyPath = Path.Combine(localStoragePath, "DataProtection");
Directory.CreateDirectory(productMediaPath);
Directory.CreateDirectory(keyPath);

var kcAdminUsername = builder.AddParameter("keycloak-admin-username", "admin");
var kcAdminPassword = builder.AddParameter("keycloak-admin-password", "admin");
var kcServiceSecret = builder.AddParameter("keycloak-admin-service-secret",
    builder.Configuration["Keycloak:AdminClientSecret"] ?? (builder.Environment.IsDevelopment()
        ? "cookiecommerce-development-admin-secret" : throw new InvalidOperationException("Configure Keycloak:AdminClientSecret.")), secret: true);
var communicationsKey = builder.AddParameter("communications-service-key",
    builder.Configuration["Communications:ServiceKey"] ?? (builder.Environment.IsDevelopment()
        ? "cookiecommerce-development-communications-key" : throw new InvalidOperationException("Configure Communications:ServiceKey for internal email delivery.")), secret: true);

var keycloak = builder.AddKeycloak(
    name: "keycloak",
    port: builder.Configuration.GetValue("DevelopmentStorage:KeycloakPort", 8080),
    adminUsername: kcAdminUsername,
    adminPassword: kcAdminPassword)
    .WithRealmImport(realmsPath)
    .WithDockerfile(Path.Combine(builder.AppHostDirectory, "..", "CookieCommerce.KeycloakEmailProvider"))
    .WithEnvironment("COOKIECOMMERCE_SERVICE_KEY", communicationsKey)
    .WithDataVolume(volumePrefix + "-keycloak-data").WithLifetime(ContainerLifetime.Persistent);

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
    .WithEnvironment("ProductMedia__StorageRootPath", productMediaPath)
    .WithEnvironment("ProductMedia__RequestPath", "/media/products")
    .WithScalarUi();
var authenticationManagementApi = builder.AddProject<CookieCommerce_AuthenticationManagement_Api>("authentication-management-api")
    .WithEnvironment("Keycloak__AdminClientSecret", kcServiceSecret)
    .WithReference(authenticationManagementDb).WaitFor(authenticationManagementDb)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithReference(rabbit).WaitFor(rabbit)
    .WithReference(customerApi)
    .WithScalarUi();

var web = builder.AddProject<CookieCommerce_Web>("cookiecommerce-web")
    .WithEnvironment("Brandbook__Enabled", brandbookEnabled.ToString())
    .WithEnvironment("DataProtection__KeyPath", keyPath)
    .WithEnvironment("Communications__ServiceKey", communicationsKey)
    .WithReference(commerceDb)
    .WaitFor(commerceDb)
    .WithReference(customerApi)
    .WithReference(productApi)
    .WithReference(authenticationManagementApi);

if (brandbookEnabled) web.WithBrandbookUi();

keycloak.WithEnvironment("COOKIECOMMERCE_TEMPLATE_URL", ReferenceExpression.Create($"{web.GetEndpoint("http")}/api/internal/email/render"));
customerApi.WithEnvironment("EmailVerificationDelivery__ExternalDeliveryEndpoint", ReferenceExpression.Create($"{web.GetEndpoint("http")}/api/internal/email/verification"))
    .WithEnvironment("EmailVerificationDelivery__VerificationBaseUrl", web.GetEndpoint("http"))
    .WithEnvironment("EmailVerificationDelivery__ServiceKey", communicationsKey);

// A separate development stack can use isolated volumes and ports without disrupting an existing store.
var portOffset = builder.Configuration.GetValue("DevelopmentStorage:PortOffset", 0);
if (portOffset != 0)
    foreach (var project in new[] { web.Resource, authenticationManagementApi.Resource, customerApi.Resource, productApi.Resource })
        foreach (var endpoint in project.Annotations.OfType<EndpointAnnotation>())
            if (endpoint.Port is { } port) endpoint.Port = checked(port + portOffset);

builder.Build().Run();
