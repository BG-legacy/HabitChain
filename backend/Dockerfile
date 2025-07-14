# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /app

# Copy solution file
COPY HabitChain.sln .

# Copy project files for better layer caching
COPY HabitChain.Domain/HabitChain.Domain.csproj HabitChain.Domain/
COPY HabitChain.Application/HabitChain.Application.csproj HabitChain.Application/
COPY HabitChain.Infrastructure/HabitChain.Infrastructure.csproj HabitChain.Infrastructure/
COPY HabitChain.WebAPI/HabitChain.WebAPI.csproj HabitChain.WebAPI/

# Restore dependencies
RUN dotnet restore HabitChain.WebAPI/HabitChain.WebAPI.csproj

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet build HabitChain.WebAPI/HabitChain.WebAPI.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish HabitChain.WebAPI/HabitChain.WebAPI.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 9.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Set the working directory
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Expose the port the app runs on
EXPOSE 8080

# Add health check
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables for production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=""
ENV ASPNETCORE_Kestrel__Limits__MaxConcurrentConnections=100
ENV ASPNETCORE_Kestrel__Limits__MaxConcurrentUpgradedConnections=100
ENV ASPNETCORE_Kestrel__Limits__MaxRequestBodySize=52428800
ENV ASPNETCORE_Kestrel__Limits__RequestHeadersTimeout=00:00:30
ENV ASPNETCORE_Kestrel__Limits__KeepAliveTimeout=00:00:02

# Run the application
ENTRYPOINT ["dotnet", "HabitChain.WebAPI.dll"] 