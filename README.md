# MVVMFluent

MVVMFluent is a lightweight .NET library that helps you build MVVM view models with fluent property setters, declarative command builders, and optional validation support. The library ships as a compiled NuGet package (targeting `netstandard2.0`) with a clean interface-based API that separates implementation details from the public contract.

## Features
- **Fluent property setters** &mdash; Compose `Changing`, `Changed`, and `Notify` behaviors with a fluent API before committing values back to the backing store.
- **Interface-based design** &mdash; Work with `IFluentSetter<T>` and `IValidationFluentSetter<T>` interfaces instead of concrete implementations, making your code more testable and maintainable.
- **Command builders** &mdash; Generate `IFluentCommand` and `IFluentCommand<T>` instances directly from your view model, keeping command wiring and `CanExecute` logic close to the properties that depend on them.
- **Async command support** &mdash; Use `IAsyncFluentCommand` / `IAsyncFluentCommand<T>` to handle cancellable asynchronous work, expose an auto-wired `CancelCommand`, and surface progress updates through `INotifyPropertyChanged`.
- **Validation pipeline** &mdash; Opt-in to `ValidationViewModelBase` to compose validation rules (such as `HasValue` or custom `Validate` callbacks) that keep the `Errors` collection and `HasErrors` flag in sync with your UI.
- **Extended validation helpers** &mdash; Reference `MVVMFluent.ValidationExtensions` for ready-to-use rules like `IsEmail`, `IsUrl`, `HasLengthBetween`, and date or range guards.
- **Deterministic cleanup** &mdash; View model, command, and builder implementations implement `IDisposable` where appropriate to avoid self-referencing leaks when commands are re-evaluated or builders are cached.

## Installation
Install the package from NuGet just like any other binary dependency:

```bash
dotnet add package MVVMFluent
```

The package embeds the project README and license so IDE package managers surface the latest documentation.

## Architecture

The library follows a clean interface-based architecture:

- **Interfaces** (`MVVMFluent.Interfaces`) - Public contracts for fluent setters, commands, and validation
- **Commands** (`MVVMFluent.Commands`) - Internal implementations of `FluentCommand` and `AsyncFluentCommand`
- **Builders** (`MVVMFluent.Builders`) - Internal implementations of fluent setter builders
- **Validation** (`MVVMFluent.Validation`) - Validation-specific builders and rules

When you use `When()` or `Do()` methods, they return interfaces (`IFluentSetter<T>`, `IFluentCommand`, etc.) rather than concrete types, allowing for better testability and encapsulation.

## Usage
### Fluent property setters
`ViewModelBase` provides the `When` helper to build rich property setters without repetitive boilerplate. The method returns an `IFluentSetter<T>` interface:

```csharp
public class MyViewModel : ViewModelBase
{
    public string? Name
    {
        get => Get<string?>();
        set => When(value)
            .Changing(newValue => Console.WriteLine($"Changing to {newValue}"))
            .Changed(newValue => Console.WriteLine($"Changed to {newValue}"))
            .Notify(SaveCommand)
            .Notify(nameof(FullName))
            .Set();
    }

    public string FullName => $"{Name} Surname";
}
```

The builder caches itself per-property, so subsequent setter invocations reuse the configured pipeline while updating the pending value.

### Commands
Create commands directly from your view model via the `Do` helpers. Commands are cached per property and automatically expose `RaiseCanExecuteChanged`. All command methods return interfaces (`IFluentCommand`, `IFluentCommand<T>`, `IAsyncFluentCommand`, or `IAsyncFluentCommand<T>`):

```csharp
public class MyViewModel : ViewModelBase
{
    public IFluentCommand SaveCommand => Do(Save)
        .If(() => !string.IsNullOrEmpty(Name));

    private void Save()
    {
        // Persist data
    }
}
```

For parameterized scenarios, use `IFluentCommand<T>` by calling `Do<T>`:

```csharp
public IFluentCommand<string> HelpCommand => Do<string>(ShowDialog);

private void ShowDialog(string? input)
{
    MessageBox.Show(input);
}
```

### Asynchronous commands
`IAsyncFluentCommand` wraps cancellable asynchronous work and keeps UI bindings informed about execution state:

```csharp
public class LoaderViewModel : ViewModelBase
{
    public IAsyncFluentCommand LoadCommand => Do(async token =>
        {
            for (var i = 0; i < 10; i++)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(100, token);
                LoadCommand.ReportProgress(i + 1, 10);
            }
        })
        .Handle(ex => Console.WriteLine($"Load failed: {ex.Message}"));

    public IFluentCommand CancelLoadCommand => LoadCommand.CancelCommand;
}
```

Bindings can observe the `IsRunning`, `Progress`, and `CancelCommand` members exposed by the async command.

### Validation
Derive from `ValidationViewModelBase` to wire validation rules directly into your setters. The `When` method returns an `IValidationFluentSetter<T>` interface. `HasValue()` optionally accepts a custom error message so you can surface friendly text when the bound property is empty:

```csharp
public class ContactViewModel : ValidationViewModelBase
{
    public string? Email
    {
        get => Get<string?>();
        set => When(value)
            .HasValue("Email is required")
            .Validate(address => address?.Contains('@') == true, "Email must contain '@'")
            .Notify(SaveCommand)
            .Set();
    }

    public bool CanSave => !HasErrors;

    public IFluentCommand SaveCommand => Do(Save).If(() => CanSave);

    private void Save()
    {
        // Save contact information
    }
}
```

Validation errors propagate to the `Errors` collection, enabling XAML data binding to display aggregated messages.

#### Extended validation extensions
Install the optional `MVVMFluent.ValidationExtensions` package to get a catalog of reusable validation helpers that extend `IValidationFluentSetter<T>`:

```bash
dotnet add package MVVMFluent.ValidationExtensions
```

The extensions provide expressive guards for common scenarios with sensible default messages and optional overrides:

```csharp
public class RegistrationViewModel : ValidationViewModelBase
{
    public string? Email
    {
        get => Get<string?>();
        set => When(value)
            .HasValue("Email is required")
            .IsEmail()
            .HasLengthBetween(5, 100)
            .Notify(RegisterCommand)
            .Set();
    }

    public int? Age
    {
        get => Get<int?>();
        set => When(value)
            .IsMinimumAge(18, "You must be at least 18 years old.")
            .IsAgeBetween(18, 120)
            .Notify(RegisterCommand)
            .Set();
    }

    public DateTime? AppointmentDate
    {
        get => Get<DateTime?>();
        set => When(value)
            .IsDateInFuture("Please choose a future date.")
            .IsDateBetween(DateTime.Today, DateTime.Today.AddYears(1))
            .Set();
    }

    public IFluentCommand RegisterCommand => Do(Register).IfValid(nameof(Email), nameof(Age));

    private void Register()
    {
        // Perform registration
    }
}
```

Additional helpers cover URL validation (`IsUrl`), numeric comparisons (`IsGreaterThan`, `IsInRange`), and pattern-based checks (`MatchesPattern`).

#### Command validation helpers
`CommandValidationExtensions` provides `IfValid` helpers for both synchronous and asynchronous commands so they only execute when
specified properties are error-free:

```csharp
public class ContactViewModel : ValidationViewModelBase
{
    public string? Email
    {
        get => Get<string?>();
        set => When(value)
            .HasValue("Email is required")
            .Validate(address => address?.Contains('@') == true, "Email must contain '@'")
            .Notify(SaveCommand)
            .Set();
    }

    public IFluentCommand SaveCommand => Do(Save).IfValid(nameof(Email));

    private void Save()
    {
        // Save contact information
    }
}
```

The extensions enforce that the owning view model derives from `ValidationViewModelBase` and throw meaningful exceptions when the
validated properties are missing or contain errors.

## API Design

MVVMFluent follows an interface-based design principle:

- **View Models** work with interfaces: `IFluentSetter<T>`, `IValidationFluentSetter<T>`, `IFluentCommand`, `IAsyncFluentCommand`
- **Implementations** are internal to the library in specific namespaces:
  - `MVVMFluent.Commands` - Command implementations
  - `MVVMFluent.Builders` - Builder implementations
  - `MVVMFluent.Validation` - Validation-specific implementations
- **Extensions** operate on interfaces, making them composable and testable

This separation ensures your view models depend on stable contracts rather than implementation details.

## Contributing
Contributions are welcome! Feel free to open issues or pull requests to improve this library.

## License
MVVMFluent is available under the MIT License. See `LICENSE.txt` for details.
