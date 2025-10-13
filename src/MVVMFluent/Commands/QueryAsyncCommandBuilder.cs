using MVVMFluent.Interfaces;
using MVVMFluent.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Commands;

/// <summary>
/// Provides a fluent API for configuring an asynchronous command that sends a query through an <see cref="IQueryDispatcher"/>.
/// </summary>
/// <typeparam name="TResult">The type of result produced by the query.</typeparam>
public sealed class QueryAsyncCommandBuilder<TResult>
{
    private readonly AsyncFluentCommand _command;
    private readonly IValidationFluentSetterViewModel _owner;
    private readonly Func<IQuery<TResult>> _queryFactory;
    private readonly IQueryDispatcher _dispatcher;
    private readonly List<Func<bool>> _predicates = new();
    private readonly List<Func<IQuery<TResult>, bool>> _queryPredicates = new();
    private readonly List<string> _validationProperties = new();
    private readonly List<Func<CancellationToken>> _tokenProviders = new();
    private readonly List<Action<Exception>> _syncExceptionHandlers = new();
    private readonly List<Func<Exception, Task>> _asyncExceptionHandlers = new();
    private TimeSpan? _timeout;
    private Func<TResult, Task>? _onResultAsync;
    private IQuery<TResult>? _cachedQuery;
    private bool _hasCachedQuery;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryAsyncCommandBuilder{TResult}"/> class.
    /// </summary>
    internal QueryAsyncCommandBuilder(IValidationFluentSetterViewModel owner, Func<IQuery<TResult>> queryFactory, IQueryDispatcher dispatcher)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _queryFactory = queryFactory ?? throw new ArgumentNullException(nameof(queryFactory));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _command = AsyncFluentCommand.Do((_, token) => ExecuteInternalAsync(token), owner);
        _command.If(_ => EvaluateCanExecute());
        _command.Handle(OnException);
    }

    /// <summary>
    /// Gets the command built by this builder.
    /// </summary>
    internal AsyncFluentCommand Command => _command;

    /// <summary>
    /// Adds a predicate that must evaluate to <see langword="true"/> before the command can execute.
    /// </summary>
    /// <param name="predicate">The predicate to evaluate.</param>
    /// <returns>The current builder instance.</returns>
    public QueryAsyncCommandBuilder<TResult> If(Func<bool> predicate)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (_command.IsBuilt)
        {
            return this;
        }

        _predicates.Add(predicate);
        _command.RaiseCanExecuteChanged();
        return this;
    }

    /// <summary>
    /// Adds a query-aware predicate that must evaluate to <see langword="true"/> before the command can execute.
    /// </summary>
    /// <param name="predicate">The predicate to evaluate with the query instance.</param>
    /// <returns>The current builder instance.</returns>
    public QueryAsyncCommandBuilder<TResult> If(Func<IQuery<TResult>, bool> predicate)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (_command.IsBuilt)
        {
            return this;
        }

        _queryPredicates.Add(predicate);
        _command.RaiseCanExecuteChanged();
        return this;
    }

    /// <summary>
    /// Adds a validation gate that requires the specified properties to be free of validation errors before execution.
    /// </summary>
    /// <param name="propertyNames">The property names that must be valid.</param>
    /// <returns>The current builder instance.</returns>
    public QueryAsyncCommandBuilder<TResult> IfValid(params string[] propertyNames)
    {
        EnsurePropertyNames(propertyNames);

        if (_command.IsBuilt)
        {
            return this;
        }

        foreach (var propertyName in propertyNames)
        {
            if (!_validationProperties.Contains(propertyName))
            {
                _validationProperties.Add(propertyName);
            }
        }

        _command.RaiseCanExecuteChanged();
        return this;
    }

    /// <summary>
    /// Links an external cancellation token to the command execution.
    /// </summary>
    /// <param name="tokenProvider">The provider that supplies the cancellation token.</param>
    /// <returns>The current builder instance.</returns>
    public QueryAsyncCommandBuilder<TResult> CancelWith(Func<CancellationToken> tokenProvider)
    {
        if (tokenProvider == null)
        {
            throw new ArgumentNullException(nameof(tokenProvider));
        }

        if (_command.IsBuilt)
        {
            return this;
        }

        _tokenProviders.Add(tokenProvider);
        return this;
    }

    /// <summary>
    /// Configures the command to cancel automatically when the specified timeout elapses.
    /// </summary>
    /// <param name="duration">The timeout duration.</param>
    /// <returns>The current builder instance.</returns>
    public QueryAsyncCommandBuilder<TResult> CancelWithin(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Timeout must be greater than zero.");
        }

        if (_command.IsBuilt)
        {
            return this;
        }

        _timeout = duration;
        return this;
    }

    /// <summary>
    /// Executes the supplied callback when the query returns a result.
    /// </summary>
    /// <param name="onResult">The callback to invoke.</param>
    /// <returns>The configured asynchronous command.</returns>
    public IAsyncFluentCommand Then(Action<TResult> onResult)
    {
        if (onResult == null)
        {
            throw new ArgumentNullException(nameof(onResult));
        }

        return Then(result =>
        {
            onResult(result);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Executes the supplied asynchronous callback when the query returns a result.
    /// </summary>
    /// <param name="onResultAsync">The asynchronous callback to invoke.</param>
    /// <returns>The configured asynchronous command.</returns>
    public IAsyncFluentCommand Then(Func<TResult, Task> onResultAsync)
    {
        if (onResultAsync == null)
        {
            throw new ArgumentNullException(nameof(onResultAsync));
        }

        if (_command.IsBuilt)
        {
            return _command;
        }

        _onResultAsync = onResultAsync;
        _command.MarkAsBuilt();
        return _command;
    }

    /// <summary>
    /// Registers a synchronous exception handler for the command execution.
    /// </summary>
    /// <param name="handler">The handler to invoke when an exception occurs.</param>
    /// <returns>The current builder instance.</returns>
    public QueryAsyncCommandBuilder<TResult> Handle(Action<Exception> handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        _syncExceptionHandlers.Add(handler);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous exception handler for the command execution.
    /// </summary>
    /// <param name="handler">The asynchronous handler to invoke when an exception occurs.</param>
    /// <returns>The current builder instance.</returns>
    public QueryAsyncCommandBuilder<TResult> Handle(Func<Exception, Task> handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        _asyncExceptionHandlers.Add(handler);
        return this;
    }

    private bool EvaluateCanExecute()
    {
        var canExecute = true;

        foreach (var predicate in _predicates)
        {
            if (!predicate())
            {
                canExecute = false;
                break;
            }
        }

        if (canExecute && _validationProperties.Count > 0)
        {
            canExecute = HasNoErrors(_validationProperties);
        }

        if (canExecute && _queryPredicates.Count > 0)
        {
            var query = EnsureQuery();
            canExecute = _queryPredicates.All(predicate => predicate(query));
        }

        if (!canExecute)
        {
            ClearCachedQuery();
        }

        return canExecute;
    }

    private async Task ExecuteInternalAsync(CancellationToken commandToken)
    {
        var query = ConsumeQuery();
        var tokens = new List<CancellationToken> { commandToken };

        foreach (var provider in _tokenProviders)
        {
            tokens.Add(provider());
        }

        using var timeoutCts = _timeout.HasValue ? new CancellationTokenSource(_timeout.Value) : null;
        if (timeoutCts != null)
        {
            tokens.Add(timeoutCts.Token);
        }

        using var linked = tokens.Count > 1 ? CancellationTokenSource.CreateLinkedTokenSource(tokens.ToArray()) : null;
        var executionToken = linked?.Token ?? commandToken;

        try
        {
            var result = await _dispatcher.Send(query, executionToken).ConfigureAwait(false);

            if (_onResultAsync != null)
            {
                await _onResultAsync(result).ConfigureAwait(false);
            }
        }
        finally
        {
            ClearCachedQuery();
        }
    }

    private void OnException(Exception exception)
    {
        foreach (var handler in _syncExceptionHandlers)
        {
            handler(exception);
        }

        foreach (var handler in _asyncExceptionHandlers)
        {
            var task = handler(exception);
            if (task == null)
            {
                continue;
            }

            task.ContinueWith(t => _ = t.Exception, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }

    private IQuery<TResult> EnsureQuery()
    {
        if (!_hasCachedQuery)
        {
            _cachedQuery = _queryFactory() ?? throw new InvalidOperationException("The query factory returned null.");
            _hasCachedQuery = true;
        }

        return _cachedQuery!;
    }

    private IQuery<TResult> ConsumeQuery()
    {
        var query = EnsureQuery();
        ClearCachedQuery();
        return query;
    }

    private void ClearCachedQuery()
    {
        _cachedQuery = default;
        _hasCachedQuery = false;
    }

    private bool HasNoErrors(IEnumerable<string> propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (_owner.GetFluentSetterBuilder(propertyName) is IValidationFluentSetterBuilder builder && builder.HasErrors)
            {
                return false;
            }
        }

        return true;
    }

    private static void EnsurePropertyNames(string[] propertyNames)
    {
        if (propertyNames == null)
        {
            throw new ArgumentNullException(nameof(propertyNames));
        }

        if (propertyNames.Length == 0)
        {
            throw new ArgumentException("At least one property name must be provided.", nameof(propertyNames));
        }

        foreach (var propertyName in propertyNames)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("Property names cannot be null or whitespace.", nameof(propertyNames));
            }
        }
    }
}
