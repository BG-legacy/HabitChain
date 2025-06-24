# HabitChain.Application

This project contains the application logic and service layer for the HabitChain backend. It orchestrates use cases and business processes, acting as a bridge between the domain and external layers.

## Purpose
- Implements application services, use cases, and business workflows.
- Defines Data Transfer Objects (DTOs) for communication between layers.
- Contains interfaces for service abstractions and mapping profiles.

## Main Contents
- **Services/**: Application services and use case handlers (e.g., HabitService, UserService).
- **DTOs/**: Data Transfer Objects for input/output between API and domain.
- **Mappings/**: AutoMapper profiles for mapping between entities and DTOs.
- **Interfaces/**: Abstractions for application services.

## Conventions
- Depends only on HabitChain.Domain (not on Infrastructure or WebAPI).
- Application services should be stateless and orchestrate domain logic.
- DTOs should be simple, serializable objects.
- Use AutoMapper for mapping between entities and DTOs.

## Contributing
- Add new services for new use cases or business processes.
- Define new DTOs as needed for API requests/responses.
- Update mapping profiles for new entity/DTO pairs.
- Do not add data access or API controller code here. 