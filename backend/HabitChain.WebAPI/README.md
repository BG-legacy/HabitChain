# HabitChain.WebAPI

This project exposes the HabitChain backend as a RESTful Web API using ASP.NET Core. It is the outermost layer of the Clean Architecture.

## Purpose
- Hosts the API endpoints for client applications (web, mobile, etc.).
- Handles HTTP requests, authentication, and API documentation (Swagger).
- Configures dependency injection and application startup.

## Main Contents
- **Controllers/**: API controllers for handling HTTP requests (e.g., HabitsController).
- **appsettings.json**: Application configuration (connection strings, settings).
- **Program.cs**: Application entry point and startup configuration.
- **Properties/**: Launch and debug settings.

## Conventions
- Depends on Application, Infrastructure, and Domain layers.
- Controllers should be thin: delegate business logic to application services.
- Use dependency injection for all services and repositories.
- Use Swagger for API documentation.

## Contributing
- Add new controllers for new API endpoints.
- Update configuration as needed for new features.
- Do not add business logic or data access code directly in controllers. 