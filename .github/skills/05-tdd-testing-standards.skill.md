---
name: APFMech TDD & Testing Standards Skill
description: Enforces a strict Test‑Driven Development (TDD) workflow, isolated unit testing with xUnit v3, FluentAssertions, and mocking patterns for the APFMech .NET 10 backend.
version: 1.0.0
---

# 05-tdd-testing-standards.skill.md

## Purpose

This skill mandates a **test‑first** approach to all code development within the APFMech project. It establishes the non‑negotiable rules for:

- Defining unit test specifications **before** any production code is written.
- Structuring tests using the **Arrange‑Act‑Assert (AAA)** paradigm.
- Using **xUnit v3** as the primary testing framework with async/ValueTask support.
- Employing **FluentAssertions** for expressive, human‑readable assertions.
- Adhering to strict mocking and isolation constraints – no integration tests at this layer.

All AI‑generated test code must follow these standards to guarantee high coverage, maintainability, and rapid feedback cycles.

---

## Testing Framework Standards (xUnit v3)

### 1. Core Attributes

- **`[Fact]`** – for single, deterministic unit tests.
- **`[Theory]`** – for parameterised tests using `[InlineData]`, `[MemberData]`, or `[ClassData]`.
- **`[Trait]`** – to categorise tests (e.g., `[Trait("Category", "Unit")]`, `[Trait("Feature", "WorkOrder")]`).
- **`[Skip]`** – **forbidden** unless there is an explicit, approved reason with a linked issue.

### 2. Asynchronous Patterns

- Use **`Task`**‑returning test methods (`async Task`) for async code under test.
- Use **`ValueTask`** where appropriate (e.g., test fixtures implementing `IAsyncLifetime`).
- **`IAsyncLifetime`** – for test class setup/teardown that is asynchronous:
  - `InitializeAsync()` – executed before any test in the class.
  - `DisposeAsync()` – executed after all tests in the class.
- **Do not** block on async code (`Wait()`, `Result`, `GetAwaiter().GetResult()`) – always `await`.

### 3. Naming Conventions (Behaviour‑Driven)

- All test method names must follow the pattern:  
  **`Should_<ExpectedBehavior>_When_<ConditionOrAction>`**  
  or **`<MethodName>_Should_<Expected>_When_<Condition>`**.

- **Examples**:
  - `Should_ReturnWorkOrder_When_ValidIdProvidedAsync()`
  - `CreateWorkOrder_Should_ThrowDomainException_When_InvalidData()`
  - `AuthenticateAsync_Should_ReturnToken_When_CredentialsValid()`

- The name must clearly convey the scenario, the action, and the expected outcome without needing to read the test body.

---

## The Arrange‑Act‑Assert (AAA) Paradigm

Every test must be structured into three distinct, clearly separated sections, using blank lines and comments if necessary.

### 1. Arrange
- **Setup** all required dependencies using mocks (see Mocking & Isolation).
- **Instantiate** the system under test (SUT) with the mocked dependencies.
- **Create** any input data or parameters (DTOs, commands, queries) with realistic values.
- Use local variables with meaningful names.

### 2. Act
- **Invoke** the single method or operation being tested.
- For asynchronous methods, use `await` and assign the result to a variable.
- The act should be a **single line** or a small block that clearly performs the operation.

### 3. Assert
- Use **FluentAssertions** to validate the outcome.
- Assertions must be **structural** and **human‑readable**.
- Validate the returned data, state changes, exception messages, or interactions with mocks (using `Verify`).
- For exceptions, use `FluentAssertions` `Should().ThrowAsync<T>()` or `Should().ThrowExactly<T>()`.

**Example – AAA in practice**:
```csharp
[Fact]
public async Task Should_ReturnWorkOrder_When_ValidIdProvidedAsync()
{
    // Arrange
    var workOrderId = Guid.NewGuid();
    var expected = new WorkOrder { Id = workOrderId, Title = "Test Order" };
    var mockRepo = new Mock<IWorkOrderRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
    var sut = new WorkOrderService(mockRepo.Object);

    // Act
    var result = await sut.GetWorkOrderAsync(workOrderId, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(workOrderId);
    result.Title.Should().Be("Test Order");
    mockRepo.Verify(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
}
```
- **Exception testing:**
```csharp
[Fact]
public async Task CreateWorkOrder_Should_ThrowDomainException_When_InvalidData()
{
    // Arrange
    var invalidCommand = new CreateWorkOrderCommand { Title = null };
    var sut = new WorkOrderService(/* mocks */);

    // Act & Assert
    await sut.Invoking(s => s.CreateAsync(invalidCommand, CancellationToken.None))
             .Should().ThrowAsync<DomainException>()
             .WithMessage("Work order title cannot be null.");
}
```
---
## Mocking & Isolation Constraints

- **1. Mocking Frameworks**
    - Use Moq (preferred) or NSubstitute for creating test doubles.
    - All external dependencies (repositories, services, I/O, logging, identity context) must be mocked.
    - Never use actual implementations of infrastructure (e.g., DbContext, HttpClient, file system) inside unit tests.

- **2. What Is Allowed**
    - Domain entities – can be instantiated directly (no mocking needed).
    - Value objects – can be created with test data.
    - In‑memory collections – for stubbing repository returns (e.g., new List<WorkOrder> { ... }).
    - Test‑specific helpers/factories – for building complex domain objects (e.g., TestDataBuilder).

- **3. What Is Forbidden**
    - Physical database access – no InMemoryDatabase or Sqlite in unit tests; those belong to integration tests.
    - File system – no File.ReadAllText or similar.
    - Network calls – no actual HTTP requests.
    - System time – unless mocked using TimeProvider or DateTimeOffset wrappers.
    - Static dependencies – must be refactored to use interfaces.

- **4. Verification**
    - Use Mock.Verify() to assert that certain calls were made (or not made) with expected arguments.
    - Verify interaction counts (e.g., Times.Once, Times.Never) to ensure the correct number of invocations.

- **5. Test Isolation**
    - Each test must be completely independent – no shared state between tests.
    - Use the test class constructor for common Arrange steps, but ensure that mocks are fresh per test (do not use static mocks).
    - If using IAsyncLifetime, ensure the setup does not leak state across tests.

---

These testing standards are mandatory for all unit tests written in the APFMech project. Violations will be flagged during code review and must be corrected before merging.