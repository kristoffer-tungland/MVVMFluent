using MVVMFluent.Commands;
using MVVMFluent.Queries;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MVVMFluent;

/// <summary>
/// Represents a base class for view models that support query commands.
/// </summary>
public abstract class QueryViewModelBase : ViewModelBase
{
    private readonly Dictionary<string, object> _queryBuilderStore = new();
    private IQueryDispatcher? _queryDispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryViewModelBase"/> class.
    /// </summary>
    protected QueryViewModelBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryViewModelBase"/> class with the specified query dispatcher.
    /// </summary>
    /// <param name="queryDispatcher">The dispatcher responsible for executing queries.</param>
    protected QueryViewModelBase(IQueryDispatcher queryDispatcher)
    {
        UseQueries(queryDispatcher);
    }

    /// <summary>
    /// Configures the view model to use the supplied <see cref="IQueryDispatcher"/>.
    /// </summary>
    /// <param name="queryDispatcher">The dispatcher responsible for executing queries.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="queryDispatcher"/> is <see langword="null"/>.</exception>
    protected void UseQueries(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
    }

    /// <summary>
    /// Creates a query command builder that dispatches the supplied query when executed.
    /// </summary>
    /// <typeparam name="TResult">The type of result produced by the query.</typeparam>
    /// <param name="factory">The factory used to create the query instance.</param>
    /// <param name="propertyName">The property name used for caching. Automatically provided by the compiler.</param>
    /// <returns>A <see cref="QueryAsyncCommandBuilder{TResult}"/> for configuring the command.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> or <paramref name="propertyName"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no query dispatcher has been configured.</exception>
    protected QueryAsyncCommandBuilder<TResult> Send<TResult>(Func<IQuery<TResult>> factory, [CallerMemberName] string? propertyName = null)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        EnsurePropertyName(propertyName);

        if (_queryDispatcher == null)
        {
            throw new InvalidOperationException("No IQueryDispatcher has been configured. Provide one via the constructor or call UseQueries before creating query commands.");
        }

        if (_queryBuilderStore.TryGetValue(propertyName!, out var existingBuilder))
        {
            return (QueryAsyncCommandBuilder<TResult>)existingBuilder;
        }

        var builder = new QueryAsyncCommandBuilder<TResult>(this, factory, _queryDispatcher);
        _queryBuilderStore[propertyName!] = builder;
        _commandStore[propertyName!] = builder.Command;
        return builder;
    }

    /// <summary>
    /// Releases resources used by this view model.
    /// </summary>
    protected override void DisposeInternal()
    {
        _queryBuilderStore.Clear();
        _queryDispatcher = null;

        base.DisposeInternal();
    }

    /// <summary>
    /// Ensures that a property name is available for caching purposes.
    /// </summary>
    /// <param name="propertyName">The property name captured by the compiler.</param>
    /// <exception cref="ArgumentNullException">Thrown when the property name could not be determined.</exception>
    /// <exception cref="ArgumentException">Thrown when the property name is used from a constructor.</exception>
    protected static void EnsurePropertyName(string? propertyName)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName), "Not able to determine property name.");
        }

        if (propertyName == ".ctor")
        {
            throw new ArgumentException("Property name must be provided when it is used inside a constructor.", nameof(propertyName));
        }
    }
}
