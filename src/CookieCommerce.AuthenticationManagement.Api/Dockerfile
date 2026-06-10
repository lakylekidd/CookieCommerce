FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY src/CookieCommerce.AuthenticationManagement.Api/CookieCommerce.AuthenticationManagement.Api.csproj src/CookieCommerce.AuthenticationManagement.Api/
COPY src/CookieCommerce.AuthenticationManagement.Application/CookieCommerce.AuthenticationManagement.Application.csproj src/CookieCommerce.AuthenticationManagement.Application/
COPY src/CookieCommerce.AuthenticationManagement.Domain/CookieCommerce.AuthenticationManagement.Domain.csproj src/CookieCommerce.AuthenticationManagement.Domain/
COPY src/CookieCommerce.AuthenticationManagement.Infrastructure/CookieCommerce.AuthenticationManagement.Infrastructure.csproj src/CookieCommerce.AuthenticationManagement.Infrastructure/
COPY src/CookieCommerce.CookieMassTransit/CookieCommerce.CookieMassTransit.csproj src/CookieCommerce.CookieMassTransit/
COPY src/CookieCommerce.DomainBuildingBlocks/CookieCommerce.DomainBuildingBlocks.csproj src/CookieCommerce.DomainBuildingBlocks/
COPY src/CookieCommerce.ServiceDefaults/CookieCommerce.ServiceDefaults.csproj src/CookieCommerce.ServiceDefaults/
RUN dotnet restore "src/CookieCommerce.AuthenticationManagement.Api/CookieCommerce.AuthenticationManagement.Api.csproj"

COPY . .
WORKDIR /build/src/CookieCommerce.AuthenticationManagement.Api
RUN dotnet publish "CookieCommerce.AuthenticationManagement.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "CookieCommerce.AuthenticationManagement.Api.dll"]
