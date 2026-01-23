# Use the .NET 10 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers to leverage Docker cache
COPY CookieCommerce.Web/CookieCommerce.Web.csproj CookieCommerce.Web/

RUN dotnet restore "CookieCommerce.Web/CookieCommerce.Web.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "CookieCommerce.Web/CookieCommerce.Web.csproj" -c Release -o /app/publish \
    --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish ./

EXPOSE 80

ENTRYPOINT ["dotnet", "CookieCommerce.Web.dll"]
