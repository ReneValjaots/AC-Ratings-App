namespace Ac.Ratings.Core {
    /// <summary>
    /// The command that relays its functionality by invoking delegates.
    /// </summary>
    public class RelayCommandModern : CommandBase {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public RelayCommandModern(Action<object> execute, Func<object, bool> canExecute = null) {
            if (execute == null) {
                throw new ArgumentNullException("execute");
            }

            if (canExecute == null) {
                // no can execute provided, then always executable
                canExecute = (o) => true;
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) {
            return canExecute(parameter);
        }

        protected override void OnExecute(object parameter) {
            execute(parameter);
        }
    }
}
