FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# We stay at the root /app or /src
WORKDIR /build 

LABEL org.opencontainers.image.source=https://github.com/lakylekidd/CookieCommerce

# Install Node.js
RUN apt-get update && apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy the projects into their relative subfolders and build the solution
# This mirrors your repo structure exactly 
COPY src/CookieCommerce.Web/CookieCommerce.Web.csproj src/CookieCommerce.Web/
COPY src/CookieCommerce.ServiceDefaults/CookieCommerce.ServiceDefaults.csproj src/CookieCommerce.ServiceDefaults/
RUN dotnet restore "src/CookieCommerce.Web/CookieCommerce.Web.csproj"

# Copy everything else
COPY . .
WORKDIR "/build/src/CookieCommerce.Web"

# Install Tailwind CSS CLI locally in the container
RUN npm install tailwindcss @tailwindcss/cli
RUN ls -R wwwroot

# Clean any previous artifacts that might be confusing the discovery
# RUN dotnet clean "CookieCommerce.Web.csproj"

# 1. Standard Publish
WORKDIR "/build/src/CookieCommerce.Web"
RUN dotnet publish "CookieCommerce.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:StaticWebAssetsEnabled=true \
    /p:UseAppHost=false

# 3. Double-check the files exist before finishing the build stage
RUN ls -la /app/publish/wwwroot/_framework/blazor.web.js

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "CookieCommerce.Web.dll"]