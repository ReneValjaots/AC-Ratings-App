using System.Windows.Input;

namespace Ac.Ratings.Core {
    /// <summary>
    /// The base implementation of a command.
    /// </summary>
    public abstract class CommandBase : ICommand {
        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void OnCanExecuteChanged() {
            CommandManager.InvalidateRequerySuggested();
        }

        public virtual bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (!CanExecute(parameter)) {
                return;
            }

            OnExecute(parameter);
        }

        protected abstract void OnExecute(object parameter);
    }
}
