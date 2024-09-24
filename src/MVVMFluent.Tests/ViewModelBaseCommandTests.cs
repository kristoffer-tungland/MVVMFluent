using System.Windows.Input;

namespace MVVMFluent.Tests;

public class ViewModelBaseCommandTests
{
    class CommandTestViewModel(Action execute) : ViewModelBase
    {
        public ICommand Command => Do(execute);
    }

    [Fact]
    internal void Do_CreatesCommand()
    {
        var viewModel = new CommandTestViewModel(() => { });
        var command = viewModel.Command;
        Assert.NotNull(command);
    }

    [Fact]
    internal void Do()
    {
        var didExecute = false;
        void execute() => didExecute = true;
        var viewModel = new CommandTestViewModel(execute);
        viewModel.Command.Execute(null);
        Assert.True(didExecute);
    }

    class CommandParameterTestViewModel(Action<object?> execute) : ViewModelBase
    {
        public ICommand Command => Do(execute);
    }

    [Fact]
    internal void DoWithParameter()
    {
        object? parameter = null;
        void execute(object? obj)
        {
            parameter = obj;
        }
        var viewModel = new CommandParameterTestViewModel(execute);
        viewModel.Command.Execute("Test");
        Assert.Equal("Test", parameter);
    }

    class CommandIfTestViewModel(Action execute, Func<bool> canExecute) : ViewModelBase
    {
        public ICommand Command => Do(execute).If(canExecute);
    }

    [Fact]
    internal void DoIf()
    {
        var didExecute = false;
        void execute() => didExecute = true;
        var viewModel = new CommandIfTestViewModel(execute, () => true);

        viewModel.Command.Execute(null);

        Assert.True(didExecute);
    }

    [Fact]
    internal void DoIf_WhenCanExecuteIsFalse_CannotExecute()
    {
        bool canExecute() => false;
        void execute() { }
        var viewModel = new CommandIfTestViewModel(execute, canExecute);

        var result = viewModel.Command.CanExecute(null);

        Assert.False(result);
    }

    [Fact]
    internal void DoIf_WhenCanExecuteIsFalse_DoesNotExecute()
    {
        bool canExecute() => false;
        var didExecute = false;
        void execute() => didExecute = true;
        var viewModel = new CommandIfTestViewModel(execute, canExecute);

        viewModel.Command.Execute(null);

        Assert.False(didExecute);
    }

    class CommandIfParameterTestViewModel(Action<object?> execute, Func<object, bool> canExecute) : ViewModelBase
    {
        public ICommand Command => Do(execute).If(canExecute);
    }

    [Fact]
    internal void DoIfWithParameter()
    {
        object? parameter = null;
        void execute(object? obj) => parameter = obj;
        var viewModel = new CommandIfParameterTestViewModel(execute, _ => true);
        viewModel.Command.Execute("Test");
        Assert.Equal("Test", parameter);
    }

    [Fact]
    internal void DoIfWithParameter_WhenCanExecuteIsFalse_CannotExecute()
    {
        bool canExecute(object obj) => false;
        void execute(object? obj) { }
        var viewModel = new CommandIfParameterTestViewModel(execute, canExecute);
        var result = viewModel.Command.CanExecute(null);
        Assert.False(result);
    }

    [Fact]
    internal void DoIfWithParameter_WhenCanExecuteIsFalse_DoesNotExecute()
    {
        bool canExecute(object obj) => false;
        var didExecute = false;
        void execute(object? obj) => didExecute = true;
        var viewModel = new CommandIfParameterTestViewModel(execute, canExecute);
        viewModel.Command.Execute("Test");
        Assert.False(didExecute);
    }

    private class CommandPropertyNotifyTestViewModel(Func<bool> canExecute) : ViewModelBase
    {
        public int Property
        {
            get => Get<int>();
            set => When(value).Notify(Command).Set();
        }
        public IFluentCommand Command => Do(() => { }).If(canExecute);
    }

    [Fact]
    internal void DoCommandPropertyNotifyTestViewModel()
    {
        var didCheckCanExecute = false;
        bool canExecute()
        {
            return didCheckCanExecute = true;
        }

        var viewModel = new CommandPropertyNotifyTestViewModel(canExecute);

        viewModel.Command.CanExecuteChanged += (sender, args) =>
        {
            viewModel.Command.CanExecute(null);
        };

        viewModel.Property = 1;

        Assert.True(didCheckCanExecute);
    }

    class CommandIfCanUseLocalVariableTestViewModel(Action execute) : ViewModelBase
    {
        private readonly Action _execute = execute;

        public ICommand Command => Do(_execute);
    }

    [Fact]
    internal void DoIfCanUseLocalVariable()
    {
        var didExecute = false;
        void execute() => didExecute = true;
        var viewModel = new CommandIfCanUseLocalVariableTestViewModel(execute);
        viewModel.Command.Execute(null);
        Assert.True(didExecute);
    }
}