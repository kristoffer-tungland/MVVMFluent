namespace MVVMFluent.Tests
{
    public class CommandViewModel : ViewModelBase
    {
    }

    public class ViewModelBaseGenericsCommandTests
    {
        private readonly CommandViewModel _viewModel = new();

        [Fact]
        public void Execute_WhenCanExecuteIsTrue_CallsExecuteAction()
        {
            // Arrange
            var executeCalled = false;
            var command = FluentCommand<string>.Do(param => executeCalled = true, _viewModel)
                .If(param => true); // CanExecute always returns true

            // Act
            command.Execute("test");

            // Assert
            Assert.True(executeCalled);
        }

        [Fact]
        public void Execute_WhenCanExecuteIsFalse_DoesNotCallExecuteAction()
        {
            // Arrange
            var executeCalled = false;
            var command = FluentCommand<string>.Do(param => executeCalled = true, _viewModel)
                .If(param => false); // CanExecute always returns false

            // Act
            command.Execute("test");

            // Assert
            Assert.False(executeCalled);
        }

        [Fact]
        public void CanExecute_WhenConditionIsTrue_ReturnsTrue()
        {
            // Arrange
            var command = FluentCommand<string>.Do(param => { }, _viewModel)
                .If(param => true); // CanExecute always returns true

            // Act
            var result = command.CanExecute("test");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecute_WhenConditionIsFalse_ReturnsFalse()
        {
            // Arrange
            var command = FluentCommand<string>.Do(param => { }, _viewModel)
                .If(param => false); // CanExecute always returns false

            // Act
            var result = command.CanExecute("test");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanExecute_WhenNotSet_ReturnsTrue()
        {
            // Arrange
            var command = FluentCommand<string>.Do(param => { }, _viewModel);

            // Act
            var result = command.CanExecute("test");

            // Assert
            Assert.True(result); // Default behavior should allow execution
        }
    }
}
