# MVVMFluent

MVVMFluent is a lightweight .NET library that helps you build MVVM view models with fluent property setters, declarative command builders, and optional validation support. The library now ships as a compiled NuGet package (targeting `netstandard2.0`) with a versioned assembly name, so it can be referenced from classic applications that require distinct assembly identities.

## Features
- **Fluent property setters** &mdash; Compose `Changing`, `Changed`, and `Notify` behaviors with a fluent API before committing values back to the backing store.
- **Command builders** &mdash; Generate `FluentCommand` and `FluentCommand<T>` instances directly from your view model, keeping command wiring and `CanExecute` logic close to the properties that depend on them.
- **Async command support** &mdash; Use `AsyncFluentCommand` / `AsyncFluentCommand<T>` to handle cancellable asynchronous work, expose an auto-wired `CancelCommand`, and surface progress updates through `INotifyPropertyChanged`.
- **Validation pipeline** &mdash; Opt-in to `ValidationViewModelBase` to compose validation rules (such as `HasValue` or custom `Validate` callbacks) that keep the `Errors` collection and `HasErrors` flag in sync with your UI.
- **Deterministic cleanup** &mdash; View model, command, and builder implementations implement `IDisposable` where appropriate to avoid self-referencing leaks when commands are re-evaluated or builders are cached.

## Installation
Install the package from NuGet just like any other binary dependency:

```bash
dotnet add package MVVMFluent
```

The package embeds the project README and license so IDE package managers surface the latest documentation.

## Usage
### Fluent property setters
`ViewModelBase` provides the `When` helper to build rich property setters without repetitive boilerplate:

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
Create commands directly from your view model via the `Do` helpers. Commands are cached per property and automatically expose `RaiseCanExecuteChanged`.

```csharp
public class MyViewModel : ViewModelBase
{
    public FluentCommand SaveCommand => Do(Save)
        .If(() => !string.IsNullOrEmpty(Name));

    private void Save()
    {
        // Persist data
    }
}
```

For parameterized scenarios, use `FluentCommand<T>` by calling `Do<T>`.

### Asynchronous commands
`AsyncFluentCommand` wraps cancellable asynchronous work and keeps UI bindings informed about execution state:

```csharp
public class LoaderViewModel : ViewModelBase
{
    public AsyncFluentCommand LoadCommand => Do(async token =>
        {
            for (var i = 0; i < 10; i++)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(100, token);
                LoadCommand.ReportProgress(i + 1, 10);
            }
        })
        .Handle(ex => Console.WriteLine($"Load failed: {ex.Message}"));

    public FluentCommand CancelLoadCommand => LoadCommand.CancelCommand;
}
```

Bindings can observe the `IsRunning`, `Progress`, and `CancelCommand` members exposed by the async command.

### Validation
Derive from `ValidationViewModelBase` to wire validation rules directly into your setters. `HasValue()` optionally accepts a custom error message so you can surface friendly text when the bound property is empty:

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

    public FluentCommand SaveCommand => Do(Save).If(() => CanSave);

    private void Save()
    {
        // Save contact information
    }
}
```

Validation errors propagate to the `Errors` collection, enabling XAML data binding to display aggregated messages.

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

    public FluentCommand SaveCommand => Do(Save).IfValid(nameof(Email));

    private void Save()
    {
        // Save contact information
    }
}
```

The extensions enforce that the owning view model derives from `ValidationViewModelBase` and throw meaningful exceptions when the
validated properties are missing or contain errors.

## Contributing
Contributions are welcome! Feel free to open issues or pull requests to improve this library.

## License
MVVMFluent is available under the MIT License. See `LICENSE.txt` for details.
