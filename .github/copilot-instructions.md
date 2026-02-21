# Copilot Instructions

## High-Level Architecture

This project follows **Clean Architecture** principles with a React SPA frontend.

### Structure

- **src/EcoApi.Domain**: The core domain layer. Contains entities, value objects, and domain logic. NO dependencies on other layers.
- **src/EcoApi.Application**: The application layer. Contains business logic, interfaces, and use cases. Depends ONLY on Domain.
- **src/EcoApi.Infrastructure**: The infrastructure layer. Implements interfaces defined in Application (e.g., database access, external APIs). Depends on Application and Infrastructure.
- **src/EcoApi.Api**: The entry point (ASP.NET Core Web API). Configures services and middleware. Depends on Application and Infrastructure.
- **src/EcoApi.Web**: The frontend application (React + TypeScript + Vite).
  - Configured as an SPA proxy in `EcoApi.Api.csproj`.
  - During `dotnet publish`, frontend assets are built and copied to the API's `wwwroot`.

## Tech Stack & Implementation Decisions

### Backend (.NET)

- **API Style**: Use **Controllers** (`[ApiController]`, inheriting from `ControllerBase`), NOT Minimal APIs.
  - The current `Program.cs` Minimal API example is a placeholder.
- **Testing**: Use **xUnit** + **FluentAssertions** + **NSubstitute**.
- **Data Access**: Use **Entity Framework Core**.

### Frontend (React)

- **State Management**:
  - Use **TanStack Query (React Query)** for all server state (API fetching, caching).
  - Use **React Context** only for global UI state (e.g., theme, user session).
- **Styling**: Use **CSS Modules** (`*.module.css`) for component-specific styles to ensure modularity.
- **Testing**: Use **Vitest** + **React Testing Library**.

## Key Conventions

- **Dependency Injection**: Services are registered in `DependencyInjection.cs` within each layer (Application, Infrastructure) and called from `Program.cs`.
- **SPA Proxy**: The backend project (`EcoApi.Api`) manages the frontend development server. Do not run `npm run dev` separately unless debugging frontend isolation; prefer `dotnet run` which handles both.
- **DTOs vs Entities**: Always use DTOs for API contracts; never expose Domain entities directly.
- **Async/Await**: Use async/await for all I/O operations.

## Coding Standards

- **SOLID Principles**: Adhere strictly to SOLID principles (Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion).
- **Comments**: All code comments must be in **English**.
- **Commit Messages**: Follow [Conventional Commits](https://www.conventionalcommits.org/) format (e.g., `feat: add user login`, `fix: resolve auth timeout`, `refactor: simplify user service`).
- **Code Quality**: Prioritize conciseness and readability. Keep functions small and focused.
- **TDD**: Follow **Test-Driven Development**. Write failing tests *before* writing implementation code.
- **Best Practices**:
  - Avoid magic numbers and strings; use constants or enums.
  - Prefer composition over inheritance.
  - Ensure all new features have corresponding unit or integration tests.
  - Follow standard .NET and React naming conventions.
