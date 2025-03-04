using System.Windows.Input;

namespace Ac.Ratings.Core;

public class RelayCommand : ICommand {
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
}

public class RelayCommand<T> : ICommand {
    private readonly Action<T> _execute;

    public RelayCommand(Action<T> execute) {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) {
        return true;
    }

    public void Execute(object? parameter) {
        if (parameter is T typedParameter) {
            _execute(typedParameter);
        }
        else if (parameter == null && typeof(T).IsClass) {
            _execute(default); // Allow null for reference types
        }
        else {
            throw new ArgumentException($"Expected parameter of type {typeof(T).Name}, but got {parameter?.GetType().Name ?? "null"}");
        }
    }
}