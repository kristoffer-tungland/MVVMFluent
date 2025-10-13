# MVVMFluent.Analyzers

A Roslyn analyzer for the MVVMFluent library that enforces proper usage of fluent property setters.

## Rules

### MVVMFLUENT001: Fluent setter must end with Set()

**Severity**: Error

**Description**: Property setters using `When(value)` must end with a call to `Set()` to commit the value to the backing field.

**Why this rule exists**: The fluent setter pattern in MVVMFluent builds up a chain of operations (validation, change notifications, etc.) but doesn't actually store the value until `Set()` is called. Without `Set()`, the value is not committed and property change notifications are not triggered, leading to subtle bugs.

### Example

#### ? Incorrect (will trigger analyzer)
```csharp
private string _name;
public string Name
{
    get => _name;
    set => When(value).Validate(); // Analyzer will report an error: missing Set()!
}

public string Title
{
    get => _title;
    set => When(value).Validate(); // Analyzer will report an error: missing Set()
}
```

#### ? Correct
```csharp
private string _name;
public string Name
{
    get => _name;
    set => When(value).Validate().Set(); // Valid
}
```

### Additional Context

This analyzer is important to prevent subtle bugs when working with FluentSetter
and FluentValidationSetter in the MVVMFluent project.

## Metadata

## Metadata

### Assignees

No one assigned

### Labels

[codex](https://github.com/kristoffer-tungland/MVVMFluent/issues?q=state%3Aopen%20label%3A%22codex%22)

### Projects

No projects

### Milestone

No milestone

### Relationships

None yet

### Development

No branches or pull requests

## Installation

The analyzer is automatically included when you install the MVVMFluent NuGet package. No separate installation is required.

## How It Works

The analyzer examines all property setters in your code and looks for:

1. Calls to the `When(value)` method (from `ViewModelBase` or `ValidationViewModelBase`)
2. Checks if the method chain ends with a call to `Set()`
3. Reports an error if `Set()` is missing

The analyzer works with both expression-bodied setters and block-bodied setters:

```csharp
// Expression-bodied setter
public string Name
{
    get => Get<string>();
    set => When(value).Set(); // ?
}

// Block-bodied setter
public string Title
{
    get => Get<string>();
    set
    {
        When(value).Set(); // ?
    }
}
```

## Technical Details

- **Analyzer ID**: MVVMFLUENT001
- **Category**: Usage
- **Severity**: Error
- **Applies to**: Property setters using `When(value)` from MVVMFluent

## Building from Source

```bash
cd src/MVVMFluent.Analyzers
dotnet build
```

## Running Tests

```bash
cd src/MVVMFluent.Analyzers.Tests
dotnet test
```

## Contributing

Contributions are welcome! Please see the main [MVVMFluent repository](https://github.com/kristoffer-tungland/MVVMFluent) for contribution guidelines.

## License

This analyzer is part of the MVVMFluent project and is available under the MIT License.
