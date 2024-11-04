# MVVMFluent
MVVMFluent is a lightweight, source-only .NET library designed to simplify the MVVM pattern through fluent command and property setter APIs. It reduces boilerplate code in view models, making your codebase cleaner, more readable, and easier to maintain.

## Features
- Fluent Property Setters: Easily chain property setters with `Changed`, `Changing`, command reevaluation, and property change notifications.
- Fluent FluentCommand Setup: Define commands with `Do` methods and conditional execution via `If`.
- Generic `FluentCommand<T>`: Define commands with parameterized actions using a strongly-typed approach.
- Implements `ICommand`: Both `FluentCommand` and `FluentCommand<T>` implement the `ICommand` interface, enabling full compatibility with WPF and other MVVM frameworks.
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
                .Changing(v => Console.WriteLine($"Changing from {Name} to {v}"))
                .Changed(v => Console.WriteLine($"Changed to {v}"))
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
    public string? Age
    {
        get => Get<string?>();
        set => Set(value);
    }
}
```
Here, the `Set` method is used directly to assign the new value to the property without needing any additional configuration like `Changing` or `Changed`.

### Fluent `FluentCommand` Setup
Commands in MVVMFluent can be easily defined with `Do` methods, and you can add conditional execution using `If`.

```csharp
public class MyViewModel : ViewModelBase
{
    public FluentCommand SaveCommand => Do(Save)
                                  .If(CanSave);

    private void Save()
    {
        // Save logic here
    }

    private bool CanSave() => !string.IsNullOrEmpty(Name);
}
```

### Using `FluentCommand<T>` for Generic Commands
In cases where you need commands that accept a parameter, you can use the generic `FluentCommand<T>`. Both `FluentCommand` and `FluentCommand<T>` implement the `ICommand` interface, making them compatible with WPF or any other MVVM framework that supports `ICommand`.

```csharp
public class MyViewModel : ViewModelBase
{
    public FluentCommand<string> SaveWithMessageCommand => Do<string>(message => Save(message))
                                                     .If(message => !string.IsNullOrEmpty(message));

    private void Save(string message)
    {
        // Implement your save logic here with a message
        Console.WriteLine($"Saved: {message}");
    }
}
```
This example demonstrates how `FluentCommand<T>` can be used to handle parameterized commands with strongly-typed arguments.

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

### Asynchronous FluentCommand (`AsyncCommand`)
The AsyncCommand in the MVVMFluent library is designed to simplify asynchronous command execution with built-in support for cancellation, progress reporting, and exception handling. It allows you to execute long-running tasks without blocking the UI thread, while maintaining full control over the command's lifecycle.

**Key Features:**
- **Cancellation Support:** The `CancelCommand` property allows users to stop an ongoing asynchronous operation, making it ideal for tasks that may take a significant amount of time or need to be interrupted.
- **Execution State Tracking:** The command exposes an `IsRunning` property to indicate whether the command is currently executing, which can be bound to UI elements like progress bars or buttons to reflect the task status dynamically.
- **Progress Reporting:** The `Progress` property enables reporting task completion status, which can be updated via the ReportProgress method. This is especially useful when performing iterative tasks or tasks with multiple stages.
- **Fluent Configuration:** Similar to the rest of the MVVMFluent library, the `AsyncCommand` can be configured fluently, allowing you to chain conditions (`If()`), exception handling (`Handle()`), and cancellation logic seamlessly.

Example Usage
In your view model:

```csharp
public AsyncCommand LoadCommand => 
                 Do(LoadData)
                .If(() => CanLoad)
                .Handle(ex => HandleError(ex));

private async Task LoadData(CancellationToken cancellationToken)
{
    // Simulate a long-running task that checks for cancellation
    for (int i = 0; i < 100; i++)
    {
        // Throw if cancellation is requested
        cancellationToken.ThrowIfCancellationRequested();

        // Simulate work
        await Task.Delay(100, cancellationToken);

        // Report progress
        LoadCommand.ReportProgress(i + 1, 100);
    }
}

public bool CanLoad { get => Get(true); set => Set(value); }
```

In XAML:
```xml
<Button Content="Load" FluentCommand="{Binding LoadCommand}" />
<ProgressBar Visibility="{Binding LoadCommand.IsRunning, Converter={StaticResource BoolToVisibilityConverter}}" />
<Button Content="Cancel" FluentCommand="{Binding LoadCommand.CancelCommand}" />
```

This example demonstrates how AsyncCommand integrates with both your view model and XAML, providing a responsive and user-friendly interface for long-running or cancellable operations.

### Asynchronous FluentCommand with Parameter (`AsyncCommand<T>`)
The `AsyncCommand<T>` extends the functionality of AsyncCommand by allowing you to pass a parameter of type `T` to the asynchronous execution logic. This makes it ideal for scenarios where the command needs to act on dynamic data or context-specific parameters. Like `AsyncCommand`, it supports cancellation, progress reporting, and exception handling, while maintaining the same fluent API for configuration.

### Validation Fluent Setter (`ValidationFluentSetter<TValue>`)
The `ValidationFluentSetter<TValue>` class in the `MVVMFluent.WPF` library provides a flexible framework for implementing validation in your MVVM applications. It supports three distinct validation methods: Built-in Validation Rules allow the use of WPF’s standard validation rules, which can be added to the setter using the `Validate(params ValidationRule[] rules)` method. For example:

```csharp
public string? Input
{
    get => Get<string?>();
    set => When(value)
                .Validate(new RequiredFieldRule())
                .Notify(OkCommand)
                .Set();
}
```
**Custom Validation Functions** enable developers to define specific validation logic encapsulated in a function, linked to a user-friendly error message, through the `Validate(Func<TValue?, bool> validationFunction, string? errorMessage)` method. For instance:

```csharp
public string? Comments
{
    get => Get<string?>();
    set => When(value)
                .Validate(IsCommentValid, "Comments are required!")
                .Notify(OkCommand)
                .Set();
}

private bool IsCommentValid(string? value)
{
    return !string.IsNullOrWhiteSpace(value);
}
```
**Conditional Validation** allows for validation checks to occur only when certain conditions are met, using the `When(value)` method. Additionally, the command can be set to execute only if the validation passes by using the `IfValid()` method, ensuring that the command is invoked only with valid input. For example:

```csharp
// Remember to use .Notify(FluentCommand) on the setter
public string? Comments
{
    get => Get<string?>();
    set => When(value)
                .Validate(IsCommentValid, "Comments are required!")
                .Notify(OkCommand) // Notify the command when the property changes
                .Set();
}

private bool IsCommentValid(string? value)
{
    return !string.IsNullOrWhiteSpace(value);
}

// Example command that executes only if the Comments property is valid
public FluentCommand OkCommand => Do(() => ShowDialog(Comments)).IfValid(nameof(Comments));
```
Together, these methods enhance the user experience by providing responsive, context-sensitive validation feedback while ensuring adherence to application requirements.


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

### Window Management
The `MVVMFluent.WPF` library provides convenient interfaces for view models to manage window behaviors, including the ability to handle closing events and show dialog windows that return specific results. By implementing the `IClosableViewModel` interface, view models can control whether a window can close based on their state. You can use the `RequestCloseView` property to initiate a close action when appropriate. Additionally, the library allows view models to implement the `IResultViewModel<TResult>` interface to encapsulate a result that can be returned when a dialog is accepted. This enables seamless interaction between your view models and the UI, ensuring that user input is validated and managed efficiently.

**Example of Attaching `IClosableViewModel` to a View**:

To attach a view model to a window, you can use the `AttachToWindow` method. Here's how you would do it:

```csharp
public class MyViewModel : IClosableViewModel
{
    public Action? RequestCloseView { get; set; }

    public bool CanCloseView() => true; // Logic to determine if the window can close

    public void Close()
    {
        RequestCloseView?.Invoke(); // Call this method to request closing the window
    }
}
```
In your view code-behind, you would attach the view model like this:

```csharp
public partial class MyWindow : Window
{
    public MyWindow()
    {
        InitializeComponent();

        var viewModel = new MyViewModel();
        this.AttachToWindow(viewModel);
    }
}
```

**Example of Implementing `IResultViewModel<TResult>`**:

To show a dialog and retrieve the result, you can implement the `IResultViewModel<TResult>` interface as follows:

```csharp
public class MyDialogViewModel : IClosableViewModel, IResultViewModel<string>
{
    
    public string? Result { get => Get<string>(); set => Set(value); }

    public string? GetResult() => Result;

    public Action? RequestCloseView { get; set; }
    public bool CanCloseView() => !string.IsNullOrEmpty(Result);
    public FluentCommand CloseCommand => Do(Close);

    public void Close()
    {
        RequestCloseView?.Invoke(); // Close the dialog when the result is set
    }
}
```
To show the dialog and retrieve the result:

```csharp
string? result = window.ShowDialog<string>(new MyDialogViewModel());
```
This approach streamlines dialog management and allows for robust validation before closing windows, enhancing the user experience in your applications

### Complete Example
Combining everything, here's an example of fluent property setters, generic commands, disposal, and using default values together:

```csharp
using MVVMFluent.WPF;

namespace MVVMFluent.Demo
{
    public class MyViewModel : ValidationViewModelBase, IClosableViewModel, IResultViewModel<string?>
    {
        public string? Name
        {
            get => Get<string?>();
            set => When(value)
                .Required()
                .Changing(newVal => Console.WriteLine($"Changing to {newVal}"))
                .Changed((oldVal, newVal) => Console.WriteLine($"Changed from {oldVal} to {newVal}"))
                .Notify(SaveCommand)
                .Notify(nameof(FullName))
                .Set();
        }

        public string FullName => $"{Name} SurName";

        public int Counter
        {
            get => Get(10);
            set => Set(value);
        }

        public string? Comments
        {
            get => Get<string?>();
            set => When(value)
                .Validate(IsCommentValid, "Comments are required!")
                .Notify(LoadCommand)
                .Set();
        }
        private bool IsCommentValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public AsyncCommand LoadCommand =>
                Do(LoadData)
                .If(() => Comments?.Contains("Load") == true)
                .Handle(HandleError);

        public FluentCommand SaveCommand =>
                Do(Save)
                .IfValid(nameof(Name))
                .If(() => !string.IsNullOrEmpty(Name));

        public FluentCommand CloseCommand => Do(() => RequestCloseView?.Invoke()).If(CanCloseView);
        public Action? RequestCloseView { get; set; }
        public bool CanCloseView()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return true;
            }

            return false;
        }

        private async Task LoadData(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 100; i++)
            {
                // Simulate work and check for cancellation
                await Task.Delay(100, cancellationToken);

                // Report progress
                LoadCommand.ReportProgress(i + 1, 100);
                Console.WriteLine($"Progress: {i + 1}%");
            }

            Console.WriteLine("Data Loaded");
        }

        private void Save()
        {
            // Save data
        }

        private void HandleError(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        public string? GetResult()
        {
            return Name;
        }
    }
}
```
In this complete example:
- The `Name` property triggers the `SaveCommand` and notifies the `FullName` property on change.
- The `Counter` property has a default value of `10`.
- The `SaveWithMessageCommand` is a generic command (`FluentCommand<string>`) that accepts a string parameter.
- The `Close()` method is provided to dispose of resources when the view model is no longer needed.

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing
Contributions are welcome! Feel free to open issues or pull requests to improve this library.
