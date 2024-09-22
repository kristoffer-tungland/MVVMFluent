# MVVMFluent
MVVMFluent is a lightweight, source-only .NET library designed to simplify the MVVM pattern through fluent command and property setter APIs. It reduces boilerplate code in view models, making your codebase cleaner, more readable, and easier to maintain.

## Features
- Fluent Property Setters: Easily chain property setters with `OnChanged`, `OnChanging`, command reevaluation, and property change notifications.
- Fluent Command Setup: Define commands with `Do` methods and conditional execution via `If`.
- Generic `Command<T>`: Define commands with parameterized actions using a strongly-typed approach.
- Implements `ICommand`: Both `Command` and `Command<T>` implement the `ICommand` interface, enabling full compatibility with WPF and other MVVM frameworks.
- Source-Only Package: Integrates directly into your project as source code, minimizing dependencies.
- Global Namespace Integration: Fully qualified `global::` namespaces for all system types and dependencies.
- Disposal Management: Manage and clean up resources with built-in `Dispose` functionality.

## Installation
Since MVVMFluent is a source-only NuGet package, it must be installed as a source code package. You can install it via the NuGet Package Manager or the .NET CLI:

```bash
dotnet add package MVVMFluent
```

## Usage
Fluent Property Setters with `When`

```csharp
public class MyViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>();
        set => When(value)
                .OnChanging(v => Console.WriteLine($"Changing from {Name} to {v}"))
                .OnChanged(v => Console.WriteLine($"Changed to {v}"))
                .Notify(SaveCommand)
                .Set();
    }
}
```
This approach uses the `When` method to set the new value, enabling you to configure actions before and after the property changes, and notify commands.

### Simplified `Get`/`Set` without `When`
You can also set properties directly using the `Set` method without configuring additional actions.

```csharp
public class MyViewModel : ViewModelBase
{
    public string Age
    {
        get => Get<string>();
        set => Set(value);
    }
}
```
Here, the `Set` method is used directly to assign the new value to the property without needing any additional configuration like `OnChanging` or `OnChanged`.

### Fluent `Command` Setup
Commands in MVVMFluent can be easily defined with `Do` methods, and you can add conditional execution using `If`.

```csharp
public class MyViewModel : ViewModelBase
{
    public Command SaveCommand => Do(() => Save())
                                  .If(() => CanSave);

    private void Save()
    {
        // Save logic here
    }

    private bool CanSave => !string.IsNullOrEmpty(Name);
}
```

### Using `Command<T>` for Generic Commands
In cases where you need commands that accept a parameter, you can use the generic `Command<T>`. Both `Command` and `Command<T>` implement the `ICommand` interface, making them compatible with WPF or any other MVVM framework that supports `ICommand`.

```csharp
public class MyViewModel : ViewModelBase
{
    public Command<string> SaveWithMessageCommand => Do<string>(message => Save(message))
                                                     .If(message => !string.IsNullOrEmpty(message));

    private void Save(string message)
    {
        // Implement your save logic here with a message
        Console.WriteLine($"Saved: {message}");
    }
}
```
This example demonstrates how `Command<T>` can be used to handle parameterized commands with strongly-typed arguments.

### Using `Notify(nameof(...))` for Property Change Notifications
You can also notify multiple properties when setting a value. This is useful when other properties are derived from or depend on the value being set.

```csharp
public class MyViewModel : ViewModelBase
{
    public string FirstName
    {
        get => Get<string>();
        set => When(value)
                .Notify(nameof(FullName)) // Notify FullName when FirstName changes
                .Set();
    }

    public string LastName
    {
        get => Get<string>();
        set => When(value)
                .Notify(nameof(FullName)) // Notify FullName when LastName changes
                .Set();
    }

    public string FullName => $"{FirstName} {LastName}";
}
```
In this example, any changes to `FirstName` or `LastName` automatically notify the `FullName` property to ensure that bindings reflecting `FullName` are updated accordingly.

### Using `Get` with a Default Value
If you want to retrieve a value with a default fallback, you can use the `Get` method with a default value.

```csharp
public class MyViewModel : ViewModelBase
{
    public int Counter
    {
        get => Get(defaultValue: 10); // Can also be short like Get(10)
        set => Set(value);
    }
}
```
In this example, if the `Counter` property hasn’t been set before, the Get(10) will return 10 as the default value.

### Disposal Management
MVVMFluent provides built-in `Dispose` functionality for cleaning up resources like commands and property stores when a `ViewModelBase` is no longer needed.

For example, when a view model is no longer in use, you can call `Dispose()` to release any resources:

```csharp
public class MyViewModel : ViewModelBase
{
    public void Close()
    {
        // Call Dispose when the view model is no longer needed
        Dispose();
    }
}
```
The `Dispose()` method automatically clears the command store, property store, and unsubscribes from the `PropertyChanged` event.

### Complete Example
Combining everything, here's an example of fluent property setters, generic commands, disposal, and using default values together:

```csharp
public class MyViewModel : ViewModelBase
{
    public string Name
    {
        get => Get<string>();
        set => When(value)
                .OnChanging(v => Console.WriteLine($"Changing from {Name} to {v}"))
                .OnChanged(v => Console.WriteLine($"Changed to {v}"))
                .Notify(SaveCommand)
                .Notify(nameof(FullName))
                .Set();
    }

    public string FullName => $"{Name}";

    public int Counter
    {
        get => Get(10);
        set => Set(value);
    }

    public Command SaveCommand => Do(() => Save())
                                  .If(() => CanSave);

    public Command<string> SaveWithMessageCommand => Do<string>(message => Save(message))
                                                     .If(message => !string.IsNullOrEmpty(message));

    private void Save()
    {
        Console.WriteLine("Saved");
    }

    private void Save(string message)
    {
        Console.WriteLine($"Saved: {message}");
    }

    private bool CanSave => !string.IsNullOrEmpty(Name);

    public void Close()
    {
        Dispose(); // Properly dispose of resources when the view model is no longer needed
    }
}
```
In this complete example:
- The `Name` property triggers the `SaveCommand` and notifies the `FullName` property on change.
- The `Counter` property has a default value of `10`.
- The `SaveWithMessageCommand` is a generic command (`Command<string>`) that accepts a string parameter.
- The `Close()` method is provided to dispose of resources when the view model is no longer needed.

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing
Contributions are welcome! Feel free to open issues or pull requests to improve this library.
