using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ac.Ratings.Core {
    /// <summary>
    /// The base implementation of the INotifyPropertyChanged contract.
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
            if (!object.Equals(storage, value)) {
                storage = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }
    }
}
