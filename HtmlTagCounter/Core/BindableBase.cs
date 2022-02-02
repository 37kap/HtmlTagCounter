using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;

namespace HtmlTagCounter.Core
{
    /// <summary>
    /// Base class that implements the INotifyPropertyChanged interface.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        readonly WeakEventManager _propertyChangedEventManager = new WeakEventManager();

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChangedEventManager.AddEventHandler(value);
            remove => _propertyChangedEventManager.RemoveEventHandler(value);
        }

        /// <summary>
        /// Sets property new value and notifies about it.
        /// /// </summary>
        /// <param name="backingStore">Reference to source</param>
        /// <param name="value">New value</param>
        /// <param name="onChanged">On changed action</param>
        /// <param name="propertyname">Changed property name</param>
        protected void SetProperty<T>(ref T backingStore, in T value, in System.Action onChanged = null, [CallerMemberName] in string propertyname = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyname);
        }

        private void OnPropertyChanged([CallerMemberName] in string propertyName = "")
        {
            _propertyChangedEventManager.RaiseEvent(this, new PropertyChangedEventArgs(propertyName), nameof(INotifyPropertyChanged.PropertyChanged));
        }
    }
}
