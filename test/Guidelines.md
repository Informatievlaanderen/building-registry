# Base Registries Unit Testing Guidelines

## Introduction
This document outlines the guidelines for writing unit tests in C# following the "Given-When-Then" principle and encourages the use of the builder pattern for test data creation.

## Given-When-Then Principle
We adhere to the "Given-When-Then" principle to structure our unit tests. The test method naming follows the format:
- **Given**: Describes the initial conditions.
- **When**: Describes the action or scenario.
- **Then**: Describes the expected outcome.

### Folder Structure
- Tests related to "When" should be organized within folders that represent the action or scenario, e.g., "WhenProposingStreetName."

### Class Structure
- Each test class should correspond to the "Given" part, e.g., "GivenMunicipality."

### Method Naming
- Test method names should follow the format: "With{Condition}_Then{Outcome}," where:
  - "With" represents the condition being set up.
  - "Then" represents the expected outcome of the test.

## Builder Pattern for Test Data
We encourage the use of the builder pattern to create test data. This approach provides a cleaner and more flexible way to set up test scenarios.

### Test Data Builders
- Create separate test data builder classes or methods for each class under test.
- Builders should have clear and expressive methods for configuring the object's state.
- Default values being set by the builder should be documented in a summary
