using MVVMFluent.Queries;

namespace MVVMFluent;

/// <summary>
/// Represents a base class for view models that support fluent property setters and query commands.
/// </summary>
public abstract class FluentViewModelBase : QueryViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentViewModelBase"/> class.
    /// </summary>
    protected FluentViewModelBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentViewModelBase"/> class with the specified query dispatcher.
    /// </summary>
    /// <param name="queryDispatcher">The dispatcher responsible for executing queries.</param>
    protected FluentViewModelBase(IQueryDispatcher queryDispatcher)
        : base(queryDispatcher)
    {
    }
}
