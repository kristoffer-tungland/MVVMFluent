using MVVMFluent.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Tests;

public class AsyncFluentCommandTests
{
    [Fact]
    public async Task ExecuteAsync_SetsIsRunningWhileTaskRuns()
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = AsyncFluentCommand.Do(() => tcs.Task, owner: null);

        var execution = command.ExecuteAsync(null);

        await WaitForConditionAsync(() => command.IsRunning);
        Assert.True(command.IsRunning);
        Assert.False(command.IsCancellationRequested);

        tcs.SetResult(null);
        await execution;

        Assert.False(command.IsRunning);
        Assert.False(command.IsCancellationRequested);
    }

    [Fact]
    public async Task CancelCommand_CancelsRunningTask()
    {
        var command = AsyncFluentCommand.Do((_, token) => Task.Delay(TimeSpan.FromSeconds(10), token), owner: null);

        var execution = command.ExecuteAsync(null);
        await WaitForConditionAsync(() => command.IsRunning);

        Assert.True(command.CancelCommand.CanExecute(null));

        command.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => execution);

        Assert.False(command.IsRunning);
        Assert.False(command.IsCancellationRequested);
    }

    [Fact]
    public async Task CancelCommand_RaisesCanExecuteChangedWhenRunningStateChanges()
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = AsyncFluentCommand.Do(() => tcs.Task, owner: null);

        var cancelCommand = command.CancelCommand;
        var canExecuteStates = new List<bool>();
        cancelCommand.CanExecuteChanged += (_, _) => canExecuteStates.Add(cancelCommand.CanExecute(null));

        Assert.False(cancelCommand.CanExecute(null));

        var execution = command.ExecuteAsync(null);

        await WaitForConditionAsync(() => command.IsRunning);
        await WaitForConditionAsync(() => canExecuteStates.Count >= 1);

        Assert.Contains(true, canExecuteStates);

        tcs.SetResult(null);
        await execution;

        await WaitForConditionAsync(() => canExecuteStates.Count >= 2);
        Assert.False(canExecuteStates[^1]);
    }

    [Fact]
    public async Task CancelWith_CancelsWhenLinkedTokenCancels()
    {
        var externalCts = new CancellationTokenSource();
        var command = AsyncFluentCommand
            .Do((_, token) => Task.Delay(TimeSpan.FromSeconds(10), token), owner: null)
            .CancelWith(() => externalCts.Token);

        var execution = command.ExecuteAsync(null);

        await WaitForConditionAsync(() => command.IsRunning);

        externalCts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => execution);

        Assert.False(command.IsRunning);
    }

    [Fact]
    public async Task CancelWithin_CancelsAfterTimeout()
    {
        var command = AsyncFluentCommand
            .Do((_, token) => Task.Delay(TimeSpan.FromSeconds(10), token), owner: null)
            .CancelWithin(TimeSpan.FromMilliseconds(50));

        var execution = command.ExecuteAsync(null);

        await Assert.ThrowsAsync<TaskCanceledException>(() => execution);

        Assert.False(command.IsRunning);
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(2));

        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("The condition was not met within the allotted time.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }
    }
}
