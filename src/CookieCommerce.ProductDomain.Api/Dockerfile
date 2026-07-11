# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CookieCommerce.ProductDomain.Api/CookieCommerce.ProductDomain.Api.csproj", "CookieCommerce.ProductDomain.Api/"]
RUN dotnet restore "./CookieCommerce.ProductDomain.Api/CookieCommerce.ProductDomain.Api.csproj"
COPY . .
WORKDIR "/src/CookieCommerce.ProductDomain.Api"
RUN dotnet build "./CookieCommerce.ProductDomain.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CookieCommerce.ProductDomain.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CookieCommerce.ProductDomain.Api.dll"]
