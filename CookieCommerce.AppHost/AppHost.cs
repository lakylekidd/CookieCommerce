var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CookieCommerce>("cookiecommerce");

builder.Build().Run();
