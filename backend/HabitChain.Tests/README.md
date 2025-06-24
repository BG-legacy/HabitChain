# HabitChain Tests

This project contains comprehensive unit tests for the HabitChain application services.

## Test Structure

### Test Base Class
- `TestBase.cs` - Provides common setup and utilities for all service tests
  - AutoFixture configuration with AutoMoq customization
  - AutoMapper setup with MappingProfile
  - Helper methods for creating test entities
  - Mock UserManager setup

### Service Tests

#### AuthServiceTests
Tests for the authentication service covering:
- User login with valid/invalid credentials
- User registration with validation
- Token refresh functionality
- Password change operations
- User management operations
- Last login tracking

#### HabitServiceTests
Tests for the habit management service covering:
- CRUD operations for habits
- Retrieving habits by user ID
- Active habit filtering
- Habit frequency validation
- Error handling for invalid operations

#### BadgeServiceTests
Tests for the badge management service covering:
- CRUD operations for badges
- Badge type validation
- Active badge filtering
- Error handling for invalid operations

#### CheckInServiceTests
Tests for the check-in service covering:
- CRUD operations for check-ins
- Date range queries
- Check-in validation by date
- User and habit-specific queries

#### EncouragementServiceTests
Tests for the encouragement service covering:
- CRUD operations for encouragements
- User-specific queries (sent/received)
- Unread encouragement filtering
- Encouragement type validation
- Mark as read functionality

#### HabitEntryServiceTests
Tests for the habit entry service covering:
- CRUD operations for habit entries
- Date range queries
- Entry validation by date
- Notes handling

#### UserBadgeServiceTests
Tests for the user badge service covering:
- CRUD operations for user badges
- Badge earning validation
- User-specific badge queries
- Habit-specific badge queries

## Test Dependencies

- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **AutoFixture** - Test data generation
- **AutoFixture.AutoMoq** - AutoMoq integration
- **AutoFixture.Xunit2** - xUnit integration

## Running Tests

### From Command Line
```bash
# Navigate to the backend directory
cd backend

# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~AuthServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~LoginAsync_WithValidCredentials_ReturnsAuthResponse"
```

### From Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer (Test > Test Explorer)
3. Build the solution
4. Run all tests or specific tests from the Test Explorer

### From VS Code
1. Install the .NET Core Test Explorer extension
2. Open the test project
3. Use the Test Explorer panel to run tests

## Test Coverage

The tests cover:
- ✅ Happy path scenarios
- ✅ Error conditions and edge cases
- ✅ Input validation
- ✅ Business logic validation
- ✅ Repository interactions
- ✅ AutoMapper functionality
- ✅ Service method contracts

## Test Patterns

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
- **Arrange**: Set up test data and mock dependencies
- **Act**: Execute the method under test
- **Assert**: Verify the expected outcomes

### Mocking Strategy
- Repository interfaces are mocked to isolate service logic
- UserManager is mocked for authentication tests
- JWT service is mocked for token operations
- AutoMapper is configured with real mapping profiles

### Test Data Generation
- AutoFixture generates test data automatically
- Helper methods create realistic test entities
- Test data is isolated and doesn't affect other tests

## Best Practices

1. **Test Isolation**: Each test is independent and doesn't rely on other tests
2. **Descriptive Names**: Test method names clearly describe the scenario being tested
3. **Single Responsibility**: Each test focuses on one specific behavior
4. **Meaningful Assertions**: Assertions verify the actual behavior, not implementation details
5. **Mock Verification**: Verify that repository methods are called with correct parameters
6. **Edge Cases**: Tests cover both success and failure scenarios

## Adding New Tests

When adding new tests:

1. Follow the existing naming convention: `MethodName_Scenario_ExpectedResult`
2. Use the `TestBase` class for common setup
3. Mock all external dependencies
4. Test both positive and negative scenarios
5. Verify repository method calls when appropriate
6. Use `[Theory]` and `[InlineData]` for parameterized tests
7. Use `[Fact]` for single scenario tests

## Example Test Structure

```csharp
[Fact]
public async Task MethodName_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    var input = CreateTestInput();
    _mockRepository.Setup(x => x.Method(input))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _service.MethodAsync(input);

    // Assert
    result.Should().NotBeNull();
    result.Property.Should().Be(expectedValue);
    _mockRepository.Verify(x => x.Method(input), Times.Once);
}
```

## Continuous Integration

The tests are designed to run in CI/CD pipelines:
- No external dependencies (databases, APIs)
- Fast execution
- Deterministic results
- Clear pass/fail indicators

## Troubleshooting

### Common Issues

1. **AutoMapper Configuration**: Ensure MappingProfile is properly configured
2. **Mock Setup**: Verify all required mock setups are in place
3. **Async/Await**: Ensure proper async/await usage in test methods
4. **Test Data**: Check that test data is properly initialized

### Debugging Tests

1. Use `Debugger.Break()` in test methods
2. Run tests in debug mode
3. Check mock verification failures
4. Verify AutoMapper mappings 