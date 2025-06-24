# HabitChain.Domain

This project contains the core business logic and domain model for the HabitChain application. It is the innermost layer of the Clean Architecture and should have no dependencies on other projects.

## Purpose
- Defines the core entities, value objects, enums, and interfaces that represent the business rules and logic of HabitChain.
- Serves as the foundation for all other layers (Application, Infrastructure, WebAPI).
- Enforces business invariants and domain-driven design principles.

## Main Contents
- **Entities/**: Core business objects such as User, Habit, CheckIn, Badge, UserBadge, Encouragement. All entities inherit from `BaseEntity` (with `Id`, `CreatedAt`, `UpdatedAt`).
- **Enums/**: Enumerations for domain concepts (e.g., HabitFrequency, BadgeType, EncouragementType).
- **Interfaces/**: Abstractions for repositories and domain services, to be implemented in other layers.

## Conventions
- No dependencies on infrastructure, application, or external libraries (except for basic .NET types).
- All business rules and invariants should be enforced here.
- Use `Guid` for all entity IDs.
- Keep entities and value objects immutable where possible.

## Contributing
- Add new domain concepts as entities, value objects, or enums.
- Update interfaces for new repository or service abstractions.
- Do not add data access, API, or framework-specific code here. 