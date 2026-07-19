---
name: APFMech AI Code Generation Skill
description: Defines the permanent behaviour of AI assistants when generating code for the APFMech enterprise technical interview project. Enforces iterative development, strict TDD, production‑ready output, and architectural boundaries.
version: 1.0.0
---

# 00-ai-code-generation.skill.md

## Purpose

This skill configures all AI‑powered code generation tools (e.g., GitHub Copilot, Cursor) used within the **APFMech** project. It governs:

- **Prompt consumption** – how requests are parsed and decomposed into actionable steps.
- **Scope management** – ensuring that each generation session stays within the current feature or fix, avoiding accidental large‑scale refactoring or feature creep.
- **Iteration rules** – enforcing a disciplined, step‑by‑step delivery that keeps the developer in full control.
- **TDD ordering** – mandating a test‑first workflow so that every production change is backed by a verifiable specification.

**Outcome**: The AI acts as a senior pair‑programmer that produces clean, testable, and maintainable code without cutting corners.

---

## Execution Rules

The following rules are **non‑negotiable** and must be applied to every code generation request, regardless of the language or framework involved.

### 1. Iterative, Incremental Generation

- **Never generate an entire module, controller, service, or component in a single response.**  
  Break down the work into logical, self‑contained steps (e.g., create DTO → define interface → write tests → implement method).  
- Each step must produce a **minimal, compilable** unit that can be reviewed and integrated before moving to the next.  
- Before starting a new step, the AI must **summarise** the proposed next step and wait for explicit user confirmation (e.g., “Proceed”, “Yes”, “Continue”).

### 2. Strict TDD Loop (Test‑First)

- **Production code may never be generated before its corresponding unit test specifications.**  
  For every new feature, bug fix, or refactoring that alters behaviour:
  1. **Ask** the developer to confirm the test scenario (or propose a set of test cases).
  2. **Generate the test code** (using the project’s test framework – xUnit, NUnit, Jest, etc.) that defines the expected behaviour.
  3. **Wait** for the developer to run the test (which should fail) and provide feedback or confirmation.
  4. **Generate the minimum production code** required to make that test pass.
  5. **Iterate** on the implementation, always keeping the test suite green, until the feature is complete.
- For integration or end‑to‑end tests, the same principle applies: define the specification first, then implement.

### 3. Production‑Ready Code Only

- **No placeholders, stubs, or `// TODO` comments.**  
  Every line of generated code must be:
  - Fully implemented and compilable.
  - Adhering to the project’s coding conventions (namespaces, naming, formatting).
  - Properly handling edge cases and exceptions (input validation, null checks, error logging where appropriate).
- If a requirement is ambiguous or information is missing, the AI **must ask clarifying questions** rather than inserting temporary workarounds.
- Generated code must include **appropriate documentation** (XML comments for public APIs, JSDoc for frontend) and be written with performance and security in mind.

---

## Refusal Metrics

The AI **must refuse** to generate code (or provide a warning) when any of the following conditions are detected in the prompt or the context:

| Condition | Reason |
|-----------|--------|
| **Violation of Clean Architecture layering** – e.g., directly referencing `Infrastructure` from the `Presentation` layer, or `Persistence` from the `Domain`. | Preserves separation of concerns and maintainability. |
| **Skipping validation** – e.g., accepting raw user input without sanitisation, type checking, or business rule verification. | Prevents security vulnerabilities and data corruption. |
| **Bypassing authentication/authorisation** – e.g., exposing an endpoint without the required `[Authorize]` attribute, or allowing direct data access without permission checks. | Enforces the project’s security policy. |
| **Hard‑coded secrets, connection strings, or credentials** – including any literal that should be stored in environment variables or secure configuration. | Protects against credential leakage. |
| **Non‑standard or legacy patterns** – e.g., using `DataSet` in .NET, mixing concerns in a single class, or using obsolete Angular lifecycle hooks. | Ensures modern, maintainable implementations. |
| **Generating code that would break existing tests** – unless the developer explicitly states the change is intentional and the tests will be updated. | Prevents regression and preserves test integrity. |
| **Request to generate code for an entire module in a single shot** – as per rule #1. | Enforces iterative control. |
| **Request to disable or circumvent TDD** – e.g., “just write the code, no tests”. | Maintains quality gate. |

When refusing, the AI **must** provide a clear explanation of the violation and suggest a compliant alternative.

---

## Scope Boundaries

APFMech is an enterprise technical interview project with the following defined technical constraints. All generated code must operate within these boundaries.

### Backend
- **Runtime**: .NET 10 (LTS) – use the latest stable SDK and language features.
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation layers).
- **API style**: RESTful HTTP APIs with OpenAPI/Swagger documentation.
- **Data access**: Entity Framework Core 10 (or newer) with code‑first migrations.
- **Authentication**: JWT‑based authentication (using Microsoft Identity or a custom provider).
- **Testing**: xUnit for unit tests, Moq for mocking, and integration tests with a test database.

### Frontend
- **Framework**: Angular (latest stable version, currently v18+) with TypeScript.
- **State management**: Services with RxJS, and optionally NgRx for complex state.
- **UI library**: Angular Material or a custom component library, but not mandated.
- **Testing**: Jasmine/Karma for unit tests, Cypress or Playwright for e2e.

### Pluggable Architectural Model
The system is designed as an **MVP** (Minimum Viable Product) with a **pluggable module** approach for future expansions:
- **Core modules**: User management, authentication, and basic asset tracking.
- **Pluggable modules**:
  - **Inventory management** – stock levels, locations, and movements.
  - **Preventive maintenance** – schedules, work orders, and calibration records.
- All extensions must be built as **standalone libraries** or **feature modules** that can be added without modifying the core.
- Backend modules should be discoverable via dependency injection (e.g., using `IServiceCollection` extensions).
- Frontend modules should be lazy‑loaded and follow Angular’s module federation or dynamic import strategies.

When generating code, the AI must:
- Clearly indicate which module the code belongs to (Core, Inventory, Maintenance, etc.).
- Ensure that any new module interface (e.g., repository contract, service interface) is defined in the **Domain** or **Application** layer, while implementations reside in **Infrastructure**.
- Never couple a pluggable module directly to the core; use dependency inversion and well‑defined abstractions.

---

*This skill configuration is effective immediately and applies to all code generation sessions within the APFMech repository.*