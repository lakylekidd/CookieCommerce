FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy ONLY the projects needed for the Web app
# (Add any other dependencies here if the build fails later)
COPY CookieCommerce.Web/CookieCommerce.Web.csproj CookieCommerce.Web/
COPY CookieCommerce.ServiceDefaults/CookieCommerce.ServiceDefaults.csproj CookieCommerce.ServiceDefaults/

# 2. Restore specifically for the Web project
# This will follow the project references and restore ServiceDefaults too
RUN dotnet restore "CookieCommerce.Web/CookieCommerce.Web.csproj"

# 3. Copy everything else
COPY . .

# 4. Publish
WORKDIR "/src/CookieCommerce.Web"
RUN dotnet publish "CookieCommerce.Web.csproj" -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "CookieCommerce.Web.dll"]