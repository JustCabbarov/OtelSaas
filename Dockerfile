# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files for layer caching
COPY HotelQrSaaS.sln .
COPY src/HotelQrSaaS.Domain/*.csproj src/HotelQrSaaS.Domain/
COPY src/HotelQrSaaS.Application/*.csproj src/HotelQrSaaS.Application/
COPY src/HotelQrSaaS.Infrastructure/*.csproj src/HotelQrSaaS.Infrastructure/
COPY src/HotelQrSaaS.Persistence/*.csproj src/HotelQrSaaS.Persistence/
COPY src/HotelQrSaaS.WebApi/*.csproj src/HotelQrSaaS.WebApi/

# Restore dependencies
RUN dotnet restore src/HotelQrSaaS.WebApi/HotelQrSaaS.WebApi.csproj

# Copy all source files and assets
COPY src/ src/
COPY assets/ assets/

# Build and publish WebApi in Release mode
WORKDIR /app/src/HotelQrSaaS.WebApi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Expose WebApi port
EXPOSE 5281

# Configure ASP.NET Core environment
ENV ASPNETCORE_URLS=http://+:5281
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "HotelQrSaaS.WebApi.dll"]
