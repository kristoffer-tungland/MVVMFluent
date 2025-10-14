using MVVMFluent.Commands;
using MVVMFluent.Interfaces;
using MVVMFluent.Queries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Tests;

public class QueryCommandTests
{
    [Fact]
    public async Task Then_Receives_Typed_Result()
    {
        var dispatcher = new TestQueryDispatcher();
        dispatcher.Register<RegisterQuery, RegisterResult>((query, _) => Task.FromResult(new RegisterResult(query.Name, query.Email)));

        var viewModel = new RegistrationViewModel(dispatcher)
        {
            Name = "Ada",
            Email = "ada@example.com",
        };

        var command = viewModel.RegisterCommand;
        await command.ExecuteAsync(null);

        Assert.Equal("Welcome, Ada!", viewModel.WelcomeMessage);
    }

    [Fact]
    public void IfValid_Blocks_When_HasErrors()
    {
        var dispatcher = new TestQueryDispatcher();
        dispatcher.Register<SimpleQuery, int>((_, _) => Task.FromResult(42));

        var viewModel = new ValidationHarnessViewModel(dispatcher);

        var command = viewModel.ValidateCommand;

        Assert.False(command.CanExecute(null));

        viewModel.SetValidationState(false);
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public async Task If_QueryPredicate_Uses_Same_Instance_As_Execute()
    {
        var dispatcher = new TestQueryDispatcher();
        TrackingQuery? predicateInstance = null;
        TrackingQuery? executedInstance = null;

        dispatcher.Register<TrackingQuery, int>((query, _) =>
        {
            executedInstance = query;
            return Task.FromResult(1);
        });

        var viewModel = new PredicateViewModel(dispatcher, query =>
        {
            predicateInstance = query;
            return true;
        });

        var command = viewModel.Command;
        Assert.True(command.CanExecute(null));
        await command.ExecuteAsync(null);

        Assert.Same(predicateInstance, executedInstance);
    }

    [Fact]
    public async Task CancelWithin_Triggers_Cancellation()
    {
        var dispatcher = new TestQueryDispatcher();
        dispatcher.Register<SlowQuery, string>(async (_, token) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            return "done";
        });

        var viewModel = new CancellationViewModel(dispatcher, builder => builder.CancelWithin(TimeSpan.FromMilliseconds(50)));
        var command = viewModel.Command;

        await Assert.ThrowsAsync<TaskCanceledException>(() => command.ExecuteAsync(null));
    }

    [Fact]
    public async Task CancelWith_Links_External_Token()
    {
        var dispatcher = new TestQueryDispatcher();
        dispatcher.Register<SlowQuery, string>(async (_, token) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            return "done";
        });

        using var externalCts = new CancellationTokenSource();
        var viewModel = new CancellationViewModel(dispatcher, builder => builder.CancelWith(() => externalCts.Token));
        var command = viewModel.Command;

        var execution = command.ExecuteAsync(null);
        externalCts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => execution);
    }

    [Fact]
    public async Task Handle_Sync_And_Async_Work()
    {
        var dispatcher = new TestQueryDispatcher();
        dispatcher.Register<FaultyQuery, int>((_, _) => Task.FromException<int>(new InvalidOperationException("boom")));

        var viewModel = new ErrorHandlingViewModel(dispatcher);
        var command = viewModel.Command;

        command.Execute(null);

        await Task.Delay(50); // allow async handler to run

        Assert.Equal("boom", viewModel.LastErrorMessage);
        Assert.True(viewModel.AsyncHandlerInvoked);
    }

    [Fact]
    public void Send_Without_Dispatcher_Throws()
    {
        var viewModel = new MissingDispatcherViewModel();
        Assert.Throws<InvalidOperationException>(() => viewModel.Command);
    }

    private sealed class RegistrationViewModel : FluentViewModelBase
    {
        public RegistrationViewModel(IQueryDispatcher dispatcher)
            : base(dispatcher)
        {
        }

        public string Name
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public string Email
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public string? WelcomeMessage
        {
            get => Get<string?>();
            private set => Set(value);
        }

        public IAsyncFluentCommand RegisterCommand => Send(() => new RegisterQuery(Name, Email))
            .Then(result => WelcomeMessage = $"Welcome, {result.Name}!");
    }

    private sealed class ValidationHarnessViewModel : ValidationViewModelBase
    {
        private readonly StubValidationBuilder _builder;

        public ValidationHarnessViewModel(IQueryDispatcher dispatcher)
            : base(dispatcher)
        {
            _builder = new StubValidationBuilder(nameof(Email)) { HasErrors = true };
            AddFluentSetterBuilder(_builder);
        }

        public string Email
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public IAsyncFluentCommand ValidateCommand => Send(() => new SimpleQuery())
            .IfValid(nameof(Email))
            .Then(_ => { });

        public void SetValidationState(bool hasErrors)
        {
            _builder.HasErrors = hasErrors;
        }
    }

    private sealed class PredicateViewModel : FluentViewModelBase
    {
        private readonly Func<TrackingQuery, bool> _predicate;

        public PredicateViewModel(IQueryDispatcher dispatcher, Func<TrackingQuery, bool> predicate)
            : base(dispatcher)
        {
            _predicate = predicate;
        }

        public IAsyncFluentCommand Command => Send(() => new TrackingQuery())
            .If(query => _predicate((TrackingQuery)query))
            .Then(_ => { });
    }

    private sealed class CancellationViewModel : FluentViewModelBase
    {
        private readonly Func<QueryAsyncCommandBuilder<string>, QueryAsyncCommandBuilder<string>> _configure;

        public CancellationViewModel(IQueryDispatcher dispatcher, Func<QueryAsyncCommandBuilder<string>, QueryAsyncCommandBuilder<string>> configure)
            : base(dispatcher)
        {
            _configure = configure;
        }

        public IAsyncFluentCommand Command => _configure(Send(() => new SlowQuery()))
            .Then(_ => { });
    }

    private sealed class ErrorHandlingViewModel : FluentViewModelBase
    {
        public ErrorHandlingViewModel(IQueryDispatcher dispatcher)
            : base(dispatcher)
        {
        }

        public string? LastErrorMessage { get; private set; }

        public bool AsyncHandlerInvoked { get; private set; }

        public IAsyncFluentCommand Command => Send(() => new FaultyQuery())
            .Handle(ex => LastErrorMessage = ex.Message)
            .Handle(async ex =>
            {
                await Task.Delay(10);
                AsyncHandlerInvoked = true;
            })
            .Then(_ => { });
    }

    private sealed class MissingDispatcherViewModel : ValidationViewModelBase
    {
        public IAsyncFluentCommand Command => Send(() => new SimpleQuery()).Then(_ => { });
    }

    private sealed class StubValidationBuilder : IValidationFluentSetterBuilder
    {
        public StubValidationBuilder(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        public bool HasErrors { get; set; }

        public bool IsBuilt => true;

        public void Build()
        {
        }

        public string GetPropertyName() => PropertyName;

        public void CheckForErrors(object? value)
        {
        }

        public IEnumerable GetErrors() => Array.Empty<string>();
    }

    private sealed class TestQueryDispatcher : IQueryDispatcher
    {
        private readonly Dictionary<Type, Func<object, CancellationToken, Task<object>>> _handlers = new();

        public void Register<TQuery, TResult>(Func<TQuery, CancellationToken, Task<TResult>> handler)
            where TQuery : IQuery<TResult>
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers[typeof(TQuery)] = async (query, token) =>
            {
                var value = await handler((TQuery)query, token).ConfigureAwait(false);
                return value ?? throw new InvalidOperationException("Query handlers must not return null.");
            };
        }

        public async Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            if (!_handlers.TryGetValue(query.GetType(), out var handler))
            {
                throw new InvalidOperationException($"No handler registered for {query.GetType().Name}.");
            }

            var result = await handler(query!, cancellationToken).ConfigureAwait(false);
            return (TResult)result!;
        }
    }

    private sealed record RegisterQuery(string Name, string Email) : IQuery<RegisterResult>;

    private sealed record RegisterResult(string Name, string Email);

    private sealed record SimpleQuery() : IQuery<int>;

    private sealed record TrackingQuery() : IQuery<int>;

    private sealed record SlowQuery() : IQuery<string>;

    private sealed record FaultyQuery() : IQuery<int>;
}
