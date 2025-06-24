# HabitChain.Infrastructure

This project contains the data access and external integration logic for the HabitChain backend. It implements repository patterns and manages database interactions.

## Purpose
- Implements data persistence using Entity Framework Core and PostgreSQL.
- Provides concrete implementations for repository and service interfaces defined in the Domain layer.
- Manages database migrations and context configuration.

## Main Contents
- **Repositories/**: Implementations of repository interfaces for entities (e.g., HabitRepository, UserRepository).
- **Data/**: Entity Framework DbContext and database configuration.
- **Migrations/**: EF Core migration files for schema changes.

## Conventions
- Depends on HabitChain.Domain and HabitChain.Application.
- Use dependency injection for repository and service implementations.
- Keep infrastructure-specific logic (e.g., database, file storage, external APIs) here.
- Do not include business logic or application workflows.

## Contributing
- Add new repository implementations for new entities.
- Update DbContext and migrations for schema changes.
- Keep all data access code isolated from business logic. 