FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# We stay at the root /app or /src
WORKDIR /build 

# 1. Copy the projects into their relative subfolders
# This mirrors your repo structure exactly
COPY src/CookieCommerce.Web/CookieCommerce.Web.csproj src/CookieCommerce.Web/
COPY src/CookieCommerce.ServiceDefaults/CookieCommerce.ServiceDefaults.csproj src/CookieCommerce.ServiceDefaults/

# 2. Restore 
# Note: Path is now relative to our WORKDIR (/build)
RUN dotnet restore "src/CookieCommerce.Web/CookieCommerce.Web.csproj"

# 3. Copy everything else
COPY . .

# 4. Build and Publish
WORKDIR "/build/src/CookieCommerce.Web"
RUN dotnet publish "CookieCommerce.Web.csproj" -c Release -o /app/publish --no-restore

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "CookieCommerce.Web.dll"]